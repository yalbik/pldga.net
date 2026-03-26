using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Application.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<IEnumerable<MemberDto>> GetAllMembersAsync()
    {
        var members = await _memberRepository.GetAllAsync();
        return members.Select(MapToDto);
    }

    public async Task<MemberDto?> GetMemberByIdAsync(Guid id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        return member == null ? null : MapToDto(member);
    }

    public async Task<MemberDto> CreateMemberAsync(CreateMemberDto dto)
    {
        var member = new Member
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            IsPaid = dto.IsPaid,
            IsAdmin = dto.IsAdmin,
            RegistrationDate = DateTime.UtcNow,
            PaymentDate = dto.IsPaid ? DateTime.UtcNow : null
        };

        await _memberRepository.AddAsync(member);
        return MapToDto(member);
    }

    public async Task UpdateMemberAsync(UpdateMemberDto dto)
    {
        var member = await _memberRepository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException("Member not found.");

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.IsPaid = dto.IsPaid;
        member.IsAdmin = dto.IsAdmin;

        await _memberRepository.UpdateAsync(member);
    }

    public async Task DeleteMemberAsync(Guid id)
    {
        await _memberRepository.DeleteAsync(id);
    }

    public async Task SetPaymentStatusAsync(Guid memberId, bool isPaid)
    {
        var member = await _memberRepository.GetByIdAsync(memberId)
            ?? throw new InvalidOperationException("Member not found.");

        member.IsPaid = isPaid;
        member.PaymentDate = isPaid ? DateTime.UtcNow : null;
        await _memberRepository.UpdateAsync(member);
    }

    public async Task BulkSetPaymentStatusAsync(IEnumerable<Guid> memberIds, bool isPaid)
    {
        foreach (var id in memberIds)
        {
            await SetPaymentStatusAsync(id, isPaid);
        }
    }

    public async Task<IEnumerable<MemberDto>> GetByPaymentStatusAsync(bool isPaid)
    {
        var members = await _memberRepository.GetByPaymentStatusAsync(isPaid);
        return members.Select(MapToDto);
    }

    public static MemberDto MapToDto(Member m) => new()
    {
        Id = m.Id,
        FirstName = m.FirstName,
        LastName = m.LastName,
        Email = m.Email,
        Phone = m.Phone,
        IsPaid = m.IsPaid,
        IsAdmin = m.IsAdmin,
        RegistrationDate = m.RegistrationDate,
        PaymentDate = m.PaymentDate,
        CurrentSeasonPoints = m.CurrentSeasonPoints
    };
}
