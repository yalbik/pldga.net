using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Application.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly IEventRepository _eventRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ISiteSettingsRepository _settingsRepository;

    public LeaderboardService(
        IEventRepository eventRepository,
        IMemberRepository memberRepository,
        ISiteSettingsRepository settingsRepository)
    {
        _eventRepository = eventRepository;
        _memberRepository = memberRepository;
        _settingsRepository = settingsRepository;
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetSeasonLeaderboardAsync(int year)
    {
        var events = (await _eventRepository.GetBySeasonAsync(year))
            .Where(e => e.Status == EventStatus.Completed).ToList();
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);

        var entries = new Dictionary<Guid, LeaderboardEntryDto>();

        foreach (var evt in events)
        {
            foreach (var result in evt.Results)
            {
                if (!members.TryGetValue(result.MemberId, out var member)) continue;

                if (!entries.TryGetValue(result.MemberId, out var entry))
                {
                    entry = new LeaderboardEntryDto
                    {
                        MemberId = result.MemberId,
                        MemberName = member.FullName,
                        IsPaid = member.IsPaid,
                        EventBreakdown = new List<EventPointBreakdownDto>()
                    };
                    entries[result.MemberId] = entry;
                }

                entry.EventsPlayed++;
                entry.TotalPoints += result.PointsAwarded;
                entry.EventBreakdown.Add(new EventPointBreakdownDto
                {
                    EventId = evt.Id,
                    EventName = evt.Name,
                    EventDate = evt.Date,
                    Placement = result.Placement,
                    PointsAwarded = result.PointsAwarded
                });
            }
        }

        var ranked = entries.Values
            .OrderByDescending(e => e.TotalPoints)
            .ThenByDescending(e => e.EventsPlayed)
            .ToList();

        for (int i = 0; i < ranked.Count; i++)
        {
            ranked[i].Rank = i + 1;
        }

        return ranked;
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetTopPlayersAsync(int count)
    {
        var settings = await _settingsRepository.GetAsync();
        var leaderboard = await GetSeasonLeaderboardAsync(settings.CurrentSeasonYear);
        return leaderboard.Take(count);
    }

    public async Task<LeaderboardEntryDto?> GetPlayerStandingAsync(Guid memberId, int year)
    {
        var leaderboard = await GetSeasonLeaderboardAsync(year);
        return leaderboard.FirstOrDefault(e => e.MemberId == memberId);
    }

    public async Task RecalculateSeasonPointsAsync(int year)
    {
        var leaderboard = await GetSeasonLeaderboardAsync(year);
        foreach (var entry in leaderboard)
        {
            var member = await _memberRepository.GetByIdAsync(entry.MemberId);
            if (member != null)
            {
                member.CurrentSeasonPoints = entry.TotalPoints;
                await _memberRepository.UpdateAsync(member);
            }
        }
    }
}
