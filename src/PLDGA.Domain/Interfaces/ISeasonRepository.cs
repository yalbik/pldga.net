using PLDGA.Domain.Entities;

namespace PLDGA.Domain.Interfaces;

public interface ISeasonRepository
{
    Task<IEnumerable<Season>> GetAllAsync();
    Task<Season?> GetByYearAsync(int year);
    Task<Season?> GetCurrentAsync();
    Task AddAsync(Season season);
    Task UpdateAsync(Season season);
}
