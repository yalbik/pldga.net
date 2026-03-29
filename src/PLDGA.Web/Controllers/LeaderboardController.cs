using Microsoft.AspNetCore.Mvc;
using PLDGA.Application.Interfaces;

namespace PLDGA.Web.Controllers;

public class LeaderboardController : BaseController
{
    private readonly ILeaderboardService _leaderboardService;
    private readonly ISiteSettingsService _settingsService;

    public LeaderboardController(ILeaderboardService leaderboardService, ISiteSettingsService settingsService)
    {
        _leaderboardService = leaderboardService;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index(int? year = null)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var seasonYear = year ?? settings.CurrentSeasonYear;
        var leaderboard = await _leaderboardService.GetSeasonLeaderboardAsync(seasonYear);
        ViewData["Year"] = seasonYear;
        ViewData["CurrentYear"] = settings.CurrentSeasonYear;
        return View(leaderboard);
    }

    public async Task<IActionResult> Player(Guid id, int? year = null)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var seasonYear = year ?? settings.CurrentSeasonYear;
        var standing = await _leaderboardService.GetPlayerStandingAsync(id, seasonYear);
        if (standing == null) return NotFound();
        ViewData["Year"] = seasonYear;
        return View(standing);
    }
}
