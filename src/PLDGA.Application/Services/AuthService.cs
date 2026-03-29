using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;
using System.Security.Cryptography;

namespace PLDGA.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemberRepository _memberRepository;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public AuthService(IUserRepository userRepository, IMemberRepository memberRepository)
    {
        _userRepository = userRepository;
        _memberRepository = memberRepository;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByUsernameAsync(dto.Username);
        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid username or password." };
        }

        user.LastLoginDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return new AuthResultDto
        {
            Success = true,
            UserId = user.Id,
            MemberId = user.MemberId,
            Username = user.Username,
            IsAdmin = user.IsAdmin
        };
    }

    public async Task<AuthResultDto> RegisterAsync(CreateMemberDto dto)
    {
        var existing = await _userRepository.GetByUsernameAsync(dto.Username);
        if (existing != null)
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Username already exists." };
        }

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

        var user = new UserAccount
        {
            Username = dto.Username,
            PasswordHash = HashPassword(dto.Password),
            Email = dto.Email,
            IsAdmin = dto.IsAdmin,
            MemberId = member.Id
        };

        member.UserId = user.Id.ToString();
        await _memberRepository.UpdateAsync(member);
        await _userRepository.AddAsync(user);

        return new AuthResultDto
        {
            Success = true,
            UserId = user.Id,
            MemberId = member.Id,
            Username = user.Username,
            IsAdmin = user.IsAdmin
        };
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !VerifyPassword(currentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> AdminResetPasswordAsync(Guid memberId, string newPassword)
    {
        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null || string.IsNullOrEmpty(member.UserId))
            return false;

        if (!Guid.TryParse(member.UserId, out var userId))
            return false;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        user.PasswordHash = HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);
        var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }
}
