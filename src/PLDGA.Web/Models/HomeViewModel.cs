using PLDGA.Application.DTOs;
using PLDGA.Domain.Entities;

namespace PLDGA.Web.Models;

public class HomeViewModel
{
    public List<LeaderboardEntryDto> TopPlayers { get; set; } = new();
    public List<NewsArticleDto> RecentNews { get; set; } = new();
    public List<EventDto> UpcomingEvents { get; set; } = new();
    public List<PollDto> ActivePolls { get; set; } = new();
    public SiteSettings Settings { get; set; } = new();
}
