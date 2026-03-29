using PLDGA.Domain.Entities;

namespace PLDGA.Domain.Interfaces;

public interface IMemberRepository
{
    Task<IEnumerable<Member>> GetAllAsync();
    Task<Member?> GetByIdAsync(Guid id);
    Task<Member?> GetByUserIdAsync(string userId);
    Task<IEnumerable<Member>> GetByPaymentStatusAsync(bool isPaid);
    Task AddAsync(Member member);
    Task UpdateAsync(Member member);
    Task DeleteAsync(Guid id);
}
