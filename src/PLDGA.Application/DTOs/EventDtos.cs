using PLDGA.Domain.Entities;

namespace PLDGA.Application.DTOs;

public class EventDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public string CourseInfo { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public DateTime RegistrationDeadline { get; set; }
    public EventStatus Status { get; set; }
    public int RegisteredCount { get; set; }
    public List<EventResultDto> Results { get; set; } = new();
    public int SeasonYear { get; set; }
}

public class EventResultDto
{
    public Guid MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public bool MemberIsPaid { get; set; }
    public int Placement { get; set; }
    public int PointsAwarded { get; set; }
    public int Score { get; set; }
}

public class CreateEventDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public string CourseInfo { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public DateTime RegistrationDeadline { get; set; }
}

public class RecordResultDto
{
    public Guid MemberId { get; set; }
    public int Placement { get; set; }
    public int Score { get; set; }
}
