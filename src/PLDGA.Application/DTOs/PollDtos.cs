using PLDGA.Domain.Entities;

namespace PLDGA.Application.DTOs;

public class PollDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<PollAnswerDto> Answers { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public Guid CreatedByMemberId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public PollStatus Status { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalVotes { get; set; }
    public bool HasUserVoted { get; set; }
}

public class PollAnswerDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public double Percentage { get; set; }
}

public class CreatePollDto
{
    public string Question { get; set; } = string.Empty;
    public List<string> Answers { get; set; } = new();
    public DateTime? EndDate { get; set; }
}
