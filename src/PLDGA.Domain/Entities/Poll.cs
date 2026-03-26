namespace PLDGA.Domain.Entities;

public enum PollStatus
{
    Active,
    Closed
}

public class PollAnswer
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int VoteCount { get; set; }
}

public class PollVote
{
    public Guid MemberId { get; set; }
    public int AnswerId { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
}

public class Poll
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Question { get; set; } = string.Empty;
    public List<PollAnswer> Answers { get; set; } = new();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid CreatedByMemberId { get; set; }
    public PollStatus Status { get; set; } = PollStatus.Active;
    public List<PollVote> Votes { get; set; } = new();
    public DateTime? EndDate { get; set; }
}
