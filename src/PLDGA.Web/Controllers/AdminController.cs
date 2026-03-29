using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;
using PLDGA.Domain.Entities;

namespace PLDGA.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IMemberService _memberService;
    private readonly IEventService _eventService;
    private readonly INewsArticleService _newsService;
    private readonly ISiteSettingsService _settingsService;
    private readonly ILeaderboardService _leaderboardService;

    public AdminController(
        IMemberService memberService,
        IEventService eventService,
        INewsArticleService newsService,
        ISiteSettingsService settingsService,
        ILeaderboardService leaderboardService)
    {
        _memberService = memberService;
        _eventService = eventService;
        _newsService = newsService;
        _settingsService = settingsService;
        _leaderboardService = leaderboardService;
    }

    public async Task<IActionResult> Index()
    {
        var members = await _memberService.GetAllMembersAsync();
        var pending = await _newsService.GetPendingArticlesAsync();
        var upcoming = await _eventService.GetUpcomingEventsAsync();

        ViewData["MemberCount"] = members.Count();
        ViewData["PaidCount"] = members.Count(m => m.IsPaid);
        ViewData["UnpaidCount"] = members.Count(m => !m.IsPaid);
        ViewData["PendingArticles"] = pending.Count();
        ViewData["UpcomingEvents"] = upcoming.Count();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(SiteSettings model)
    {
        await _settingsService.SaveSettingsAsync(model);
        TempData["Success"] = "Settings saved successfully.";
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecalculatePoints()
    {
        var settings = await _settingsService.GetSettingsAsync();
        await _leaderboardService.RecalculateSeasonPointsAsync(settings.CurrentSeasonYear);
        TempData["Success"] = "Season points recalculated.";
        return RedirectToAction(nameof(Index));
    }
}
