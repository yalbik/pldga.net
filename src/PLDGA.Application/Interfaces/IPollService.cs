using PLDGA.Application.DTOs;

namespace PLDGA.Application.Interfaces;

public interface IPollService
{
    Task<IEnumerable<PollDto>> GetAllPollsAsync(Guid? currentMemberId = null);
    Task<PollDto?> GetPollByIdAsync(Guid id, Guid? currentMemberId = null);
    Task<IEnumerable<PollDto>> GetActivePollsAsync(Guid? currentMemberId = null);
    Task<PollDto> CreatePollAsync(CreatePollDto dto, Guid createdByMemberId);
    Task VoteAsync(Guid pollId, Guid memberId, int answerId);
    Task ClosePollAsync(Guid pollId);
    Task DeletePollAsync(Guid pollId);
}
