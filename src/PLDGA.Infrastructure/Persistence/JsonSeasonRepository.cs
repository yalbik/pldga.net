using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Infrastructure.Persistence;

public class JsonSeasonRepository : ISeasonRepository
{
    private readonly JsonFileStore<Season> _store;

    public JsonSeasonRepository(string dataDirectory)
    {
        _store = new JsonFileStore<Season>(dataDirectory, "seasons.json");
    }

    public async Task<IEnumerable<Season>> GetAllAsync()
    {
        return await _store.ReadAllAsync();
    }

    public async Task<Season?> GetByYearAsync(int year)
    {
        var seasons = await _store.ReadAllAsync();
        return seasons.FirstOrDefault(s => s.Year == year);
    }

    public async Task<Season?> GetCurrentAsync()
    {
        var seasons = await _store.ReadAllAsync();
        return seasons.FirstOrDefault(s => s.IsCurrent);
    }

    public async Task AddAsync(Season season)
    {
        var seasons = await _store.ReadAllAsync();
        seasons.Add(season);
        await _store.WriteAllAsync(seasons);
    }

    public async Task UpdateAsync(Season season)
    {
        var seasons = await _store.ReadAllAsync();
        var index = seasons.FindIndex(s => s.Year == season.Year);
        if (index >= 0)
        {
            seasons[index] = season;
            await _store.WriteAllAsync(seasons);
        }
    }
}
