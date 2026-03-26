using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;

namespace PLDGA.Web.Controllers;

[Authorize]
public class NewsController : BaseController
{
    private readonly INewsArticleService _newsService;
    private readonly IWebHostEnvironment _env;
    private readonly ISiteSettingsService _settingsService;

    public NewsController(INewsArticleService newsService, IWebHostEnvironment env, ISiteSettingsService settingsService)
    {
        _newsService = newsService;
        _env = env;
        _settingsService = settingsService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var articles = await _newsService.GetApprovedArticlesAsync();
        return View(articles);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id)
    {
        var article = await _newsService.GetArticleByIdAsync(id);
        if (article == null) return NotFound();
        return View(article);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateNewsArticleDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateNewsArticleDto model, IFormFile? image)
    {
        if (CurrentMemberId == null) return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Content))
        {
            ModelState.AddModelError("", "Title and content are required.");
            return View(model);
        }

        var article = await _newsService.CreateArticleAsync(model, CurrentMemberId.Value);

        if (image != null && image.Length > 0)
        {
            var settings = await _settingsService.GetSettingsAsync();
            var allowedExtensions = settings.AllowedImageFormats.Split(',').Select(f => $".{f.Trim()}").ToList();
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (allowedExtensions.Contains(ext) && image.Length <= settings.MaxImageSizeMb * 1024 * 1024)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsDir);
                var safeFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, safeFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);
                await _newsService.AddImageAsync(article.Id, $"/uploads/{safeFileName}");
            }
        }

        TempData["Success"] = "Article submitted for approval.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Pending()
    {
        var articles = await _newsService.GetPendingArticlesAsync();
        return View(articles);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _newsService.ApproveArticleAsync(id);
        TempData["Success"] = "Article approved.";
        return RedirectToAction(nameof(Pending));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id)
    {
        await _newsService.RejectArticleAsync(id);
        TempData["Success"] = "Article rejected.";
        return RedirectToAction(nameof(Pending));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _newsService.DeleteArticleAsync(id);
        TempData["Success"] = "Article deleted.";
        return RedirectToAction(nameof(Index));
    }
}
