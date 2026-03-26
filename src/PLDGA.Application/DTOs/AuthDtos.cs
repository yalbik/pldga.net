namespace PLDGA.Application.DTOs;

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid UserId { get; set; }
    public Guid MemberId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
