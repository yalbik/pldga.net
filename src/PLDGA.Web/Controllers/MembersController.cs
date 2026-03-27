using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;

namespace PLDGA.Web.Controllers;

[Authorize]
public class MembersController : BaseController
{
    private readonly IMemberService _memberService;
    private readonly ILeaderboardService _leaderboardService;
    private readonly ISiteSettingsService _settingsService;
    private readonly IAuthService _authService;

    public MembersController(IMemberService memberService, ILeaderboardService leaderboardService, ISiteSettingsService settingsService, IAuthService authService)
    {
        _memberService = memberService;
        _leaderboardService = leaderboardService;
        _settingsService = settingsService;
        _authService = authService;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(string? filter = null)
    {
        var members = filter switch
        {
            "paid" => await _memberService.GetByPaymentStatusAsync(true),
            "unpaid" => await _memberService.GetByPaymentStatusAsync(false),
            _ => await _memberService.GetAllMembersAsync()
        };
        ViewData["Filter"] = filter;
        return View(members);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateMemberDto());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMemberDto model)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
        {
            ModelState.AddModelError("", "First name and last name are required.");
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError("", "Username and password are required.");
            return View(model);
        }

        var result = await _authService.RegisterAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.ErrorMessage ?? "Failed to create member.");
            return View(model);
        }

        TempData["Success"] = "Member created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var member = await _memberService.GetMemberByIdAsync(id);
        if (member == null) return NotFound();
        var dto = new UpdateMemberDto
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Email = member.Email,
            Phone = member.Phone,
            IsPaid = member.IsPaid,
            IsAdmin = member.IsAdmin
        };
        return View(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateMemberDto model)
    {
        await _memberService.UpdateMemberAsync(model);
        TempData["Success"] = "Member updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePayment(Guid id, bool isPaid)
    {
        await _memberService.SetPaymentStatusAsync(id, isPaid);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkPay(List<Guid> memberIds)
    {
        await _memberService.BulkSetPaymentStatusAsync(memberIds, true);
        TempData["Success"] = $"{memberIds.Count} member(s) marked as paid.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _memberService.DeleteMemberAsync(id);
        TempData["Success"] = "Member deleted.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> ResetPassword(Guid id)
    {
        var member = await _memberService.GetMemberByIdAsync(id);
        if (member == null) return NotFound();
        ViewData["MemberName"] = member.FullName;
        ViewData["MemberId"] = member.Id;
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(Guid id, string newPassword, string confirmPassword)
    {
        var member = await _memberService.GetMemberByIdAsync(id);
        if (member == null) return NotFound();

        ViewData["MemberName"] = member.FullName;
        ViewData["MemberId"] = member.Id;

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            ModelState.AddModelError("", "Password is required.");
            return View();
        }
        if (newPassword.Length < 6)
        {
            ModelState.AddModelError("", "Password must be at least 6 characters.");
            return View();
        }
        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match.");
            return View();
        }

        var success = await _authService.AdminResetPasswordAsync(id, newPassword);
        if (!success)
        {
            ModelState.AddModelError("", "Failed to reset password. Member may not have a user account.");
            return View();
        }

        TempData["Success"] = $"Password reset for {member.FullName}.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    public async Task<IActionResult> Profile(Guid? id = null)
    {
        var memberId = id ?? CurrentMemberId;
        if (memberId == null) return RedirectToAction("Login", "Account");

        var member = await _memberService.GetMemberByIdAsync(memberId.Value);
        if (member == null) return NotFound();

        var settings = await _settingsService.GetSettingsAsync();
        var standing = await _leaderboardService.GetPlayerStandingAsync(memberId.Value, settings.CurrentSeasonYear);

        ViewData["Standing"] = standing;
        return View(member);
    }
}
