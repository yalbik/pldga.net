namespace PLDGA.Domain.Entities;

public enum EventStatus
{
    Upcoming,
    Active,
    Completed
}

public class EventResult
{
    public Guid MemberId { get; set; }
    public int Placement { get; set; }
    public int PointsAwarded { get; set; }
    public int Score { get; set; }
}

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public string CourseInfo { get; set; } = string.Empty;
    public int MaxParticipants { get; set; } = 72;
    public DateTime RegistrationDeadline { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Upcoming;
    public List<Guid> RegisteredMembers { get; set; } = new();
    public List<EventResult> Results { get; set; } = new();
    public int SeasonYear { get; set; }
}
