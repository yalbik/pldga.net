using PLDGA.Domain.Entities;

namespace PLDGA.Domain.Interfaces;

public interface IPollRepository
{
    Task<IEnumerable<Poll>> GetAllAsync();
    Task<Poll?> GetByIdAsync(Guid id);
    Task<IEnumerable<Poll>> GetByStatusAsync(PollStatus status);
    Task AddAsync(Poll poll);
    Task UpdateAsync(Poll poll);
    Task DeleteAsync(Guid id);
}
