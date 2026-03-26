using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Infrastructure.Persistence;

public class JsonMemberRepository : IMemberRepository
{
    private readonly JsonFileStore<Member> _store;

    public JsonMemberRepository(string dataDirectory)
    {
        _store = new JsonFileStore<Member>(dataDirectory, "members.json");
    }

    public async Task<IEnumerable<Member>> GetAllAsync()
    {
        return await _store.ReadAllAsync();
    }

    public async Task<Member?> GetByIdAsync(Guid id)
    {
        var members = await _store.ReadAllAsync();
        return members.FirstOrDefault(m => m.Id == id);
    }

    public async Task<Member?> GetByUserIdAsync(string userId)
    {
        var members = await _store.ReadAllAsync();
        return members.FirstOrDefault(m => m.UserId == userId);
    }

    public async Task<IEnumerable<Member>> GetByPaymentStatusAsync(bool isPaid)
    {
        var members = await _store.ReadAllAsync();
        return members.Where(m => m.IsPaid == isPaid);
    }

    public async Task AddAsync(Member member)
    {
        var members = await _store.ReadAllAsync();
        members.Add(member);
        await _store.WriteAllAsync(members);
    }

    public async Task UpdateAsync(Member member)
    {
        var members = await _store.ReadAllAsync();
        var index = members.FindIndex(m => m.Id == member.Id);
        if (index >= 0)
        {
            members[index] = member;
            await _store.WriteAllAsync(members);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var members = await _store.ReadAllAsync();
        members.RemoveAll(m => m.Id == id);
        await _store.WriteAllAsync(members);
    }
}
