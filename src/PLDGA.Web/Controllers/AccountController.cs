using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;
using System.Security.Claims;

namespace PLDGA.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError("", "Username and password are required.");
            return View(model);
        }

        var result = await _authService.LoginAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.ErrorMessage ?? "Login failed.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, result.Username),
            new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
            new("MemberId", result.MemberId.ToString()),
            new(ClaimTypes.Role, result.IsAdmin ? "Admin" : "User")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            ModelState.AddModelError("", "All fields are required.");
            return View();
        }

        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("", "New password and confirmation do not match.");
            return View();
        }

        if (newPassword.Length < 6)
        {
            ModelState.AddModelError("", "New password must be at least 6 characters.");
            return View();
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return RedirectToAction("Login");
        }

        var success = await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);
        if (!success)
        {
            ModelState.AddModelError("", "Current password is incorrect.");
            return View();
        }

        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction("ChangePassword");
    }
}
