using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PLDGA.Application.Interfaces;
using PLDGA.Web.Models;

namespace PLDGA.Web.Controllers;

public class HomeController : BaseController
{
    private readonly ILeaderboardService _leaderboardService;
    private readonly INewsArticleService _newsService;
    private readonly IEventService _eventService;
    private readonly IPollService _pollService;
    private readonly ISiteSettingsService _settingsService;

    public HomeController(
        ILeaderboardService leaderboardService,
        INewsArticleService newsService,
        IEventService eventService,
        IPollService pollService,
        ISiteSettingsService settingsService)
    {
        _leaderboardService = leaderboardService;
        _newsService = newsService;
        _eventService = eventService;
        _pollService = pollService;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetSettingsAsync();
        var topPlayers = await _leaderboardService.GetTopPlayersAsync(settings.TopPlayersOnHomePage);
        var news = (await _newsService.GetApprovedArticlesAsync()).Take(settings.ArticlesOnHomePage);
        var upcoming = (await _eventService.GetUpcomingEventsAsync()).Take(3);
        var polls = (await _pollService.GetActivePollsAsync(CurrentMemberId)).Take(3);

        var model = new HomeViewModel
        {
            TopPlayers = topPlayers.ToList(),
            RecentNews = news.ToList(),
            UpcomingEvents = upcoming.ToList(),
            ActivePolls = polls.ToList(),
            Settings = settings
        };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
