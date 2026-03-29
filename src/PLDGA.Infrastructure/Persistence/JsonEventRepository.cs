using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Infrastructure.Persistence;

public class JsonEventRepository : IEventRepository
{
    private readonly JsonFileStore<Event> _store;

    public JsonEventRepository(string dataDirectory)
    {
        _store = new JsonFileStore<Event>(dataDirectory, "events.json");
    }

    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        return await _store.ReadAllAsync();
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        var events = await _store.ReadAllAsync();
        return events.FirstOrDefault(e => e.Id == id);
    }

    public async Task<IEnumerable<Event>> GetBySeasonAsync(int year)
    {
        var events = await _store.ReadAllAsync();
        return events.Where(e => e.SeasonYear == year);
    }

    public async Task<IEnumerable<Event>> GetByStatusAsync(EventStatus status)
    {
        var events = await _store.ReadAllAsync();
        return events.Where(e => e.Status == status);
    }

    public async Task AddAsync(Event evt)
    {
        var events = await _store.ReadAllAsync();
        events.Add(evt);
        await _store.WriteAllAsync(events);
    }

    public async Task UpdateAsync(Event evt)
    {
        var events = await _store.ReadAllAsync();
        var index = events.FindIndex(e => e.Id == evt.Id);
        if (index >= 0)
        {
            events[index] = evt;
            await _store.WriteAllAsync(events);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var events = await _store.ReadAllAsync();
        events.RemoveAll(e => e.Id == id);
        await _store.WriteAllAsync(events);
    }
}
