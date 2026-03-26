using Moq;
using PLDGA.Application.Services;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Tests.Application;

[TestFixture]
public class LeaderboardServiceTests
{
    private Mock<IEventRepository> _eventRepo = null!;
    private Mock<IMemberRepository> _memberRepo = null!;
    private Mock<ISiteSettingsRepository> _settingsRepo = null!;
    private LeaderboardService _service = null!;
    private Member _member1 = null!;
    private Member _member2 = null!;

    [SetUp]
    public void SetUp()
    {
        _eventRepo = new Mock<IEventRepository>();
        _memberRepo = new Mock<IMemberRepository>();
        _settingsRepo = new Mock<ISiteSettingsRepository>();
        _service = new LeaderboardService(_eventRepo.Object, _memberRepo.Object, _settingsRepo.Object);

        _member1 = new Member { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith", IsPaid = true };
        _member2 = new Member { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Jones", IsPaid = true };
        _memberRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Member> { _member1, _member2 });

        _settingsRepo.Setup(r => r.GetAsync()).ReturnsAsync(new SiteSettings { CurrentSeasonYear = 2025 });
    }

    [Test]
    public async Task GetSeasonLeaderboardAsync_AggregatesPointsAcrossEvents()
    {
        var events = new List<Event>
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Event 1", Date = DateTime.UtcNow.AddDays(-10),
                Status = EventStatus.Completed, SeasonYear = 2025,
                Results = new List<EventResult>
                {
                    new() { MemberId = _member1.Id, Placement = 1, PointsAwarded = 100 },
                    new() { MemberId = _member2.Id, Placement = 2, PointsAwarded = 90 }
                }
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Event 2", Date = DateTime.UtcNow.AddDays(-5),
                Status = EventStatus.Completed, SeasonYear = 2025,
                Results = new List<EventResult>
                {
                    new() { MemberId = _member2.Id, Placement = 1, PointsAwarded = 100 },
                    new() { MemberId = _member1.Id, Placement = 3, PointsAwarded = 82 }
                }
            }
        };
        _eventRepo.Setup(r => r.GetBySeasonAsync(2025)).ReturnsAsync(events);

        var result = (await _service.GetSeasonLeaderboardAsync(2025)).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        // member2 has 190, member1 has 182
        Assert.That(result[0].MemberName, Is.EqualTo("Bob Jones"));
        Assert.That(result[0].TotalPoints, Is.EqualTo(190));
        Assert.That(result[0].EventsPlayed, Is.EqualTo(2));
        Assert.That(result[0].Rank, Is.EqualTo(1));
        Assert.That(result[1].MemberName, Is.EqualTo("Alice Smith"));
        Assert.That(result[1].TotalPoints, Is.EqualTo(182));
        Assert.That(result[1].Rank, Is.EqualTo(2));
    }

    [Test]
    public async Task GetSeasonLeaderboardAsync_ExcludesUpcomingEvents()
    {
        var events = new List<Event>
        {
            new()
            {
                Status = EventStatus.Upcoming,
                Results = new List<EventResult> { new() { MemberId = _member1.Id, PointsAwarded = 100 } }
            }
        };
        _eventRepo.Setup(r => r.GetBySeasonAsync(2025)).ReturnsAsync(events);

        var result = (await _service.GetSeasonLeaderboardAsync(2025)).ToList();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetSeasonLeaderboardAsync_IncludesEventBreakdown()
    {
        var events = new List<Event>
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Event 1", Date = new DateTime(2025, 3, 15),
                Status = EventStatus.Completed, SeasonYear = 2025,
                Results = new List<EventResult>
                {
                    new() { MemberId = _member1.Id, Placement = 1, PointsAwarded = 100 }
                }
            }
        };
        _eventRepo.Setup(r => r.GetBySeasonAsync(2025)).ReturnsAsync(events);

        var result = (await _service.GetSeasonLeaderboardAsync(2025)).ToList();

        Assert.That(result[0].EventBreakdown, Has.Count.EqualTo(1));
        Assert.That(result[0].EventBreakdown[0].EventName, Is.EqualTo("Event 1"));
        Assert.That(result[0].EventBreakdown[0].Placement, Is.EqualTo(1));
    }

    [Test]
    public async Task GetTopPlayersAsync_ReturnsLimitedResults()
    {
        var events = new List<Event>
        {
            new()
            {
                Status = EventStatus.Completed, SeasonYear = 2025,
                Results = new List<EventResult>
                {
                    new() { MemberId = _member1.Id, Placement = 1, PointsAwarded = 100 },
                    new() { MemberId = _member2.Id, Placement = 2, PointsAwarded = 90 }
                }
            }
        };
        _eventRepo.Setup(r => r.GetBySeasonAsync(2025)).ReturnsAsync(events);

        var result = (await _service.GetTopPlayersAsync(1)).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].TotalPoints, Is.EqualTo(100));
    }

    [Test]
    public async Task GetPlayerStandingAsync_Exists_ReturnsEntry()
    {
        var events = new List<Event>
        {
            new()
            {
                Status = EventStatus.Completed, SeasonYear = 2025,
                Results = new List<EventResult>
                {
                    new() { MemberId = _member1.Id, Placement = 1, PointsAwarded = 100 }
                }
            }
        };
        _eventRepo.Setup(r => r.GetBySeasonAsync(2025)).ReturnsAsync(events);

        var result = await _service.GetPlayerStandingAsync(_member1.Id, 2025);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TotalPoints, Is.EqualTo(100));
    }

    [Test]
    public async Task GetPlayerStandingAsync_NotInLeaderboard_ReturnsNull()
    {
        _eventRepo.Setup(r => r.GetBySeasonAsync(2025)).ReturnsAsync(new List<Event>());

        var result = await _service.GetPlayerStandingAsync(Guid.NewGuid(), 2025);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task RecalculateSeasonPointsAsync_UpdatesMemberPoints()
    {
        var events = new List<Event>
        {
            new()
            {
                Status = EventStatus.Completed, SeasonYear = 2025,
                Results = new List<EventResult>
                {
                    new() { MemberId = _member1.Id, Placement = 1, PointsAwarded = 100 }
                }
            }
        };
        _eventRepo.Setup(r => r.GetBySeasonAsync(2025)).ReturnsAsync(events);
        _memberRepo.Setup(r => r.GetByIdAsync(_member1.Id)).ReturnsAsync(_member1);
        _memberRepo.Setup(r => r.UpdateAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        await _service.RecalculateSeasonPointsAsync(2025);

        Assert.That(_member1.CurrentSeasonPoints, Is.EqualTo(100));
        _memberRepo.Verify(r => r.UpdateAsync(_member1), Times.Once);
    }

    [Test]
    public async Task GetSeasonLeaderboardAsync_EmptyEvents_ReturnsEmpty()
    {
        _eventRepo.Setup(r => r.GetBySeasonAsync(2025)).ReturnsAsync(new List<Event>());

        var result = (await _service.GetSeasonLeaderboardAsync(2025)).ToList();

        Assert.That(result, Is.Empty);
    }
}
