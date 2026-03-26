using PLDGA.Domain.Entities;

namespace PLDGA.Domain.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<UserAccount>> GetAllAsync();
    Task<UserAccount?> GetByIdAsync(Guid id);
    Task<UserAccount?> GetByUsernameAsync(string username);
    Task<UserAccount?> GetByMemberIdAsync(Guid memberId);
    Task AddAsync(UserAccount user);
    Task UpdateAsync(UserAccount user);
    Task DeleteAsync(Guid id);
}
