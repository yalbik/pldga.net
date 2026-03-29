using PLDGA.Domain.Entities;

namespace PLDGA.Tests.Domain;

[TestFixture]
public class PollTests
{
    [Test]
    public void NewPoll_HasDefaultValues()
    {
        var poll = new Poll();

        Assert.That(poll.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(poll.Question, Is.EqualTo(string.Empty));
        Assert.That(poll.Answers, Is.Empty);
        Assert.That(poll.Votes, Is.Empty);
        Assert.That(poll.Status, Is.EqualTo(PollStatus.Active));
        Assert.That(poll.EndDate, Is.Null);
    }

    [Test]
    public void PollAnswer_StoresProperties()
    {
        var answer = new PollAnswer { Id = 1, Text = "Option A", VoteCount = 5 };

        Assert.That(answer.Id, Is.EqualTo(1));
        Assert.That(answer.Text, Is.EqualTo("Option A"));
        Assert.That(answer.VoteCount, Is.EqualTo(5));
    }

    [Test]
    public void PollVote_StoresProperties()
    {
        var memberId = Guid.NewGuid();
        var vote = new PollVote { MemberId = memberId, AnswerId = 2 };

        Assert.That(vote.MemberId, Is.EqualTo(memberId));
        Assert.That(vote.AnswerId, Is.EqualTo(2));
        Assert.That(vote.VotedAt, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void PollStatus_HasAllValues()
    {
        Assert.That(Enum.GetValues<PollStatus>(), Has.Length.EqualTo(2));
    }
}
