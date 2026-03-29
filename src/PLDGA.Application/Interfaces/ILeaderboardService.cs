using PLDGA.Application.DTOs;

namespace PLDGA.Application.Interfaces;

public interface ILeaderboardService
{
    Task<IEnumerable<LeaderboardEntryDto>> GetSeasonLeaderboardAsync(int year);
    Task<IEnumerable<LeaderboardEntryDto>> GetTopPlayersAsync(int count);
    Task<LeaderboardEntryDto?> GetPlayerStandingAsync(Guid memberId, int year);
    Task RecalculateSeasonPointsAsync(int year);
}
