using Moq;
using PLDGA.Application.DTOs;
using PLDGA.Application.Services;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Tests.Application;

[TestFixture]
public class PollServiceTests
{
    private Mock<IPollRepository> _pollRepo = null!;
    private Mock<IMemberRepository> _memberRepo = null!;
    private PollService _service = null!;
    private Member _testMember = null!;

    [SetUp]
    public void SetUp()
    {
        _pollRepo = new Mock<IPollRepository>();
        _memberRepo = new Mock<IMemberRepository>();
        _service = new PollService(_pollRepo.Object, _memberRepo.Object);

        _testMember = new Member { Id = Guid.NewGuid(), FirstName = "Test", LastName = "User" };
        _memberRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Member> { _testMember });
    }

    [Test]
    public async Task GetAllPollsAsync_ReturnsSortedByDateDesc()
    {
        var polls = new List<Poll>
        {
            new() { Question = "Old", CreatedDate = DateTime.UtcNow.AddDays(-10), CreatedByMemberId = _testMember.Id },
            new() { Question = "New", CreatedDate = DateTime.UtcNow, CreatedByMemberId = _testMember.Id }
        };
        _pollRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(polls);

        var result = (await _service.GetAllPollsAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Question, Is.EqualTo("New"));
    }

    [Test]
    public async Task GetPollByIdAsync_Exists_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var poll = new Poll { Id = id, Question = "Test?", CreatedByMemberId = _testMember.Id };
        _pollRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(poll);

        var result = await _service.GetPollByIdAsync(id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Question, Is.EqualTo("Test?"));
    }

    [Test]
    public async Task GetPollByIdAsync_NotExists_ReturnsNull()
    {
        _pollRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Poll?)null);
        var result = await _service.GetPollByIdAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreatePollAsync_CreatesWithAnswers()
    {
        var dto = new CreatePollDto
        {
            Question = "Favorite course?",
            Answers = new List<string> { "Course A", "Course B", "Course C" },
            EndDate = DateTime.UtcNow.AddDays(7)
        };
        _pollRepo.Setup(r => r.AddAsync(It.IsAny<Poll>())).Returns(Task.CompletedTask);

        var result = await _service.CreatePollAsync(dto, _testMember.Id);

        Assert.That(result.Question, Is.EqualTo("Favorite course?"));
        Assert.That(result.Answers, Has.Count.EqualTo(3));
        Assert.That(result.Answers[0].Text, Is.EqualTo("Course A"));
        Assert.That(result.Answers[0].Id, Is.EqualTo(1));
        _pollRepo.Verify(r => r.AddAsync(It.IsAny<Poll>()), Times.Once);
    }

    [Test]
    public void CreatePollAsync_TooManyAnswers_Throws()
    {
        var dto = new CreatePollDto
        {
            Question = "Test?",
            Answers = Enumerable.Range(1, 11).Select(i => $"Answer {i}").ToList()
        };

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePollAsync(dto, _testMember.Id));
    }

    [Test]
    public void CreatePollAsync_NoAnswers_Throws()
    {
        var dto = new CreatePollDto { Question = "Test?", Answers = new List<string>() };
        Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePollAsync(dto, _testMember.Id));
    }

    [Test]
    public async Task VoteAsync_ValidVote_RecordsVote()
    {
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Status = PollStatus.Active,
            Answers = new List<PollAnswer>
            {
                new() { Id = 1, Text = "A", VoteCount = 0 },
                new() { Id = 2, Text = "B", VoteCount = 0 }
            }
        };
        _pollRepo.Setup(r => r.GetByIdAsync(poll.Id)).ReturnsAsync(poll);
        _pollRepo.Setup(r => r.UpdateAsync(It.IsAny<Poll>())).Returns(Task.CompletedTask);

        await _service.VoteAsync(poll.Id, _testMember.Id, 1);

        Assert.That(poll.Answers[0].VoteCount, Is.EqualTo(1));
        Assert.That(poll.Votes, Has.Count.EqualTo(1));
        Assert.That(poll.Votes[0].MemberId, Is.EqualTo(_testMember.Id));
    }

    [Test]
    public void VoteAsync_AlreadyVoted_Throws()
    {
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Status = PollStatus.Active,
            Answers = new List<PollAnswer> { new() { Id = 1, Text = "A" } },
            Votes = new List<PollVote> { new() { MemberId = _testMember.Id, AnswerId = 1 } }
        };
        _pollRepo.Setup(r => r.GetByIdAsync(poll.Id)).ReturnsAsync(poll);

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.VoteAsync(poll.Id, _testMember.Id, 1));
    }

    [Test]
    public void VoteAsync_PollClosed_Throws()
    {
        var poll = new Poll { Id = Guid.NewGuid(), Status = PollStatus.Closed };
        _pollRepo.Setup(r => r.GetByIdAsync(poll.Id)).ReturnsAsync(poll);

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.VoteAsync(poll.Id, _testMember.Id, 1));
    }

    [Test]
    public void VoteAsync_InvalidAnswer_Throws()
    {
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Status = PollStatus.Active,
            Answers = new List<PollAnswer> { new() { Id = 1, Text = "A" } }
        };
        _pollRepo.Setup(r => r.GetByIdAsync(poll.Id)).ReturnsAsync(poll);

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.VoteAsync(poll.Id, _testMember.Id, 99));
    }

    [Test]
    public void VoteAsync_PollNotFound_Throws()
    {
        _pollRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Poll?)null);
        Assert.ThrowsAsync<InvalidOperationException>(() => _service.VoteAsync(Guid.NewGuid(), _testMember.Id, 1));
    }

    [Test]
    public async Task ClosePollAsync_SetsStatusToClosed()
    {
        var poll = new Poll { Id = Guid.NewGuid(), Status = PollStatus.Active };
        _pollRepo.Setup(r => r.GetByIdAsync(poll.Id)).ReturnsAsync(poll);
        _pollRepo.Setup(r => r.UpdateAsync(It.IsAny<Poll>())).Returns(Task.CompletedTask);

        await _service.ClosePollAsync(poll.Id);

        Assert.That(poll.Status, Is.EqualTo(PollStatus.Closed));
    }

    [Test]
    public async Task DeletePollAsync_CallsRepository()
    {
        var id = Guid.NewGuid();
        _pollRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        await _service.DeletePollAsync(id);

        _pollRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Test]
    public async Task GetActivePollsAsync_AutoClosesExpiredPolls()
    {
        var expiredPoll = new Poll
        {
            Id = Guid.NewGuid(),
            Question = "Expired?",
            Status = PollStatus.Active,
            EndDate = DateTime.UtcNow.AddDays(-1),
            CreatedByMemberId = _testMember.Id
        };
        var activePoll = new Poll
        {
            Id = Guid.NewGuid(),
            Question = "Active?",
            Status = PollStatus.Active,
            EndDate = DateTime.UtcNow.AddDays(5),
            CreatedByMemberId = _testMember.Id
        };
        _pollRepo.Setup(r => r.GetByStatusAsync(PollStatus.Active)).ReturnsAsync(new List<Poll> { expiredPoll, activePoll });
        _pollRepo.Setup(r => r.UpdateAsync(It.IsAny<Poll>())).Returns(Task.CompletedTask);

        var result = (await _service.GetActivePollsAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Question, Is.EqualTo("Active?"));
        Assert.That(expiredPoll.Status, Is.EqualTo(PollStatus.Closed));
    }

    [Test]
    public async Task PollDto_CalculatesPercentages()
    {
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Question = "Test?",
            CreatedByMemberId = _testMember.Id,
            Answers = new List<PollAnswer>
            {
                new() { Id = 1, Text = "A", VoteCount = 3 },
                new() { Id = 2, Text = "B", VoteCount = 7 }
            }
        };
        _pollRepo.Setup(r => r.GetByIdAsync(poll.Id)).ReturnsAsync(poll);

        var result = await _service.GetPollByIdAsync(poll.Id);

        Assert.That(result!.TotalVotes, Is.EqualTo(10));
        Assert.That(result.Answers[0].Percentage, Is.EqualTo(30.0));
        Assert.That(result.Answers[1].Percentage, Is.EqualTo(70.0));
    }

    [Test]
    public async Task PollDto_HasUserVoted_TrackedCorrectly()
    {
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Question = "Test?",
            CreatedByMemberId = _testMember.Id,
            Votes = new List<PollVote> { new() { MemberId = _testMember.Id, AnswerId = 1 } },
            Answers = new List<PollAnswer> { new() { Id = 1, Text = "A", VoteCount = 1 } }
        };
        _pollRepo.Setup(r => r.GetByIdAsync(poll.Id)).ReturnsAsync(poll);

        var voted = await _service.GetPollByIdAsync(poll.Id, _testMember.Id);
        var notVoted = await _service.GetPollByIdAsync(poll.Id, Guid.NewGuid());

        Assert.That(voted!.HasUserVoted, Is.True);
        Assert.That(notVoted!.HasUserVoted, Is.False);
    }
}
