using PLDGA.Application.DTOs;

namespace PLDGA.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task<AuthResultDto> RegisterAsync(CreateMemberDto dto);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> AdminResetPasswordAsync(Guid memberId, string newPassword);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
