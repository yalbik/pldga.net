using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Infrastructure.Persistence;

public class JsonPollRepository : IPollRepository
{
    private readonly JsonFileStore<Poll> _store;

    public JsonPollRepository(string dataDirectory)
    {
        _store = new JsonFileStore<Poll>(dataDirectory, "polls.json");
    }

    public async Task<IEnumerable<Poll>> GetAllAsync()
    {
        return await _store.ReadAllAsync();
    }

    public async Task<Poll?> GetByIdAsync(Guid id)
    {
        var polls = await _store.ReadAllAsync();
        return polls.FirstOrDefault(p => p.Id == id);
    }

    public async Task<IEnumerable<Poll>> GetByStatusAsync(PollStatus status)
    {
        var polls = await _store.ReadAllAsync();
        return polls.Where(p => p.Status == status);
    }

    public async Task AddAsync(Poll poll)
    {
        var polls = await _store.ReadAllAsync();
        polls.Add(poll);
        await _store.WriteAllAsync(polls);
    }

    public async Task UpdateAsync(Poll poll)
    {
        var polls = await _store.ReadAllAsync();
        var index = polls.FindIndex(p => p.Id == poll.Id);
        if (index >= 0)
        {
            polls[index] = poll;
            await _store.WriteAllAsync(polls);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var polls = await _store.ReadAllAsync();
        polls.RemoveAll(p => p.Id == id);
        await _store.WriteAllAsync(polls);
    }
}
