using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Infrastructure.Persistence;

public class JsonUserRepository : IUserRepository
{
    private readonly JsonFileStore<UserAccount> _store;

    public JsonUserRepository(string dataDirectory)
    {
        _store = new JsonFileStore<UserAccount>(dataDirectory, "user_accounts.json");
    }

    public async Task<IEnumerable<UserAccount>> GetAllAsync()
    {
        return await _store.ReadAllAsync();
    }

    public async Task<UserAccount?> GetByIdAsync(Guid id)
    {
        var users = await _store.ReadAllAsync();
        return users.FirstOrDefault(u => u.Id == id);
    }

    public async Task<UserAccount?> GetByUsernameAsync(string username)
    {
        var users = await _store.ReadAllAsync();
        return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<UserAccount?> GetByMemberIdAsync(Guid memberId)
    {
        var users = await _store.ReadAllAsync();
        return users.FirstOrDefault(u => u.MemberId == memberId);
    }

    public async Task AddAsync(UserAccount user)
    {
        var users = await _store.ReadAllAsync();
        users.Add(user);
        await _store.WriteAllAsync(users);
    }

    public async Task UpdateAsync(UserAccount user)
    {
        var users = await _store.ReadAllAsync();
        var index = users.FindIndex(u => u.Id == user.Id);
        if (index >= 0)
        {
            users[index] = user;
            await _store.WriteAllAsync(users);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var users = await _store.ReadAllAsync();
        users.RemoveAll(u => u.Id == id);
        await _store.WriteAllAsync(users);
    }
}
