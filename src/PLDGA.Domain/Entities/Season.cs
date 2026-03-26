namespace PLDGA.Domain.Entities;

public class Season
{
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public List<Guid> EventIds { get; set; } = new();
}
