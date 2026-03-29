namespace PLDGA.Application.DTOs;

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public Guid MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public int TotalPoints { get; set; }
    public int EventsPlayed { get; set; }
    public List<EventPointBreakdownDto> EventBreakdown { get; set; } = new();
}

public class EventPointBreakdownDto
{
    public Guid EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int Placement { get; set; }
    public int PointsAwarded { get; set; }
}
