namespace PLDGA.Application.DTOs;

public class MemberDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public int CurrentSeasonPoints { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string PaymentStatusLabel => IsPaid ? "Paid" : "Unpaid";
}

public class CreateMemberDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public bool IsAdmin { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateMemberDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public bool IsAdmin { get; set; }
}
