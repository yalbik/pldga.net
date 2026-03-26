using PLDGA.Domain.Entities;

namespace PLDGA.Domain.Interfaces;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetBySeasonAsync(int year);
    Task<IEnumerable<Event>> GetByStatusAsync(EventStatus status);
    Task AddAsync(Event evt);
    Task UpdateAsync(Event evt);
    Task DeleteAsync(Guid id);
}
