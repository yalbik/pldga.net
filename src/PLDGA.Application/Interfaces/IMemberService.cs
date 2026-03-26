using PLDGA.Application.DTOs;

namespace PLDGA.Application.Interfaces;

public interface IMemberService
{
    Task<IEnumerable<MemberDto>> GetAllMembersAsync();
    Task<MemberDto?> GetMemberByIdAsync(Guid id);
    Task<MemberDto> CreateMemberAsync(CreateMemberDto dto);
    Task UpdateMemberAsync(UpdateMemberDto dto);
    Task DeleteMemberAsync(Guid id);
    Task SetPaymentStatusAsync(Guid memberId, bool isPaid);
    Task BulkSetPaymentStatusAsync(IEnumerable<Guid> memberIds, bool isPaid);
    Task<IEnumerable<MemberDto>> GetByPaymentStatusAsync(bool isPaid);
}
