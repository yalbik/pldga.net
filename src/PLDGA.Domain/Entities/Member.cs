namespace PLDGA.Domain.Entities;

public class Member
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public DateTime? PaymentDate { get; set; }
    public int CurrentSeasonPoints { get; set; }
    public string UserId { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
    public string PaymentStatusLabel => IsPaid ? "Paid" : "Unpaid";
}
