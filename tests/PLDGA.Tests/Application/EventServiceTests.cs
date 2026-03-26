using Moq;
using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;
using PLDGA.Application.Services;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Tests.Application;

[TestFixture]
public class EventServiceTests
{
    private Mock<IEventRepository> _eventRepo = null!;
    private Mock<IMemberRepository> _memberRepo = null!;
    private Mock<ISeasonRepository> _seasonRepo = null!;
    private Mock<ISiteSettingsService> _settingsService = null!;
    private EventService _service = null!;

    private Member _paidMember = null!;
    private Member _unpaidMember = null!;

    [SetUp]
    public void SetUp()
    {
        _eventRepo = new Mock<IEventRepository>();
        _memberRepo = new Mock<IMemberRepository>();
        _seasonRepo = new Mock<ISeasonRepository>();
        _settingsService = new Mock<ISiteSettingsService>();
        _service = new EventService(_eventRepo.Object, _memberRepo.Object, _seasonRepo.Object, _settingsService.Object);

        _paidMember = new Member { Id = Guid.NewGuid(), FirstName = "Paid", LastName = "Player", IsPaid = true };
        _unpaidMember = new Member { Id = Guid.NewGuid(), FirstName = "Unpaid", LastName = "Player", IsPaid = false };
        _memberRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Member> { _paidMember, _unpaidMember });
    }

    [Test]
    public async Task GetAllEventsAsync_ReturnsSortedByDateDesc()
    {
        var events = new List<Event>
        {
            new() { Name = "Old", Date = DateTime.UtcNow.AddDays(-10), SeasonYear = 2025 },
            new() { Name = "New", Date = DateTime.UtcNow.AddDays(10), SeasonYear = 2025 }
        };
        _eventRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(events);

        var result = (await _service.GetAllEventsAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("New"));
    }

    [Test]
    public async Task GetEventByIdAsync_Exists_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var evt = new Event { Id = id, Name = "Test Event" };
        _eventRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(evt);

        var result = await _service.GetEventByIdAsync(id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Test Event"));
    }

    [Test]
    public async Task GetEventByIdAsync_NotExists_ReturnsNull()
    {
        _eventRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Event?)null);
        var result = await _service.GetEventByIdAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUpcomingEventsAsync_FiltersByStatus()
    {
        var events = new List<Event>
        {
            new() { Name = "Upcoming1", Date = DateTime.UtcNow.AddDays(5) },
            new() { Name = "Upcoming2", Date = DateTime.UtcNow.AddDays(1) }
        };
        _eventRepo.Setup(r => r.GetByStatusAsync(EventStatus.Upcoming)).ReturnsAsync(events);

        var result = (await _service.GetUpcomingEventsAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("Upcoming2")); // sorted ascending
    }

    [Test]
    public async Task CreateEventAsync_CreatesAndReturns()
    {
        var dto = new CreateEventDto
        {
            Name = "New Event",
            Date = DateTime.UtcNow.AddDays(30),
            LocationName = "Park",
            MaxParticipants = 36,
            RegistrationDeadline = DateTime.UtcNow.AddDays(25)
        };
        _eventRepo.Setup(r => r.AddAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);
        _seasonRepo.Setup(r => r.GetByYearAsync(2025)).ReturnsAsync(new Season { Year = 2025 });
        _seasonRepo.Setup(r => r.UpdateAsync(It.IsAny<Season>())).Returns(Task.CompletedTask);

        var result = await _service.CreateEventAsync(dto, 2025);

        Assert.That(result.Name, Is.EqualTo("New Event"));
        Assert.That(result.MaxParticipants, Is.EqualTo(36));
        _eventRepo.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Once);
    }

    [Test]
    public async Task RegisterMemberAsync_Success()
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            MaxParticipants = 10,
            RegistrationDeadline = DateTime.UtcNow.AddDays(5)
        };
        _eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
        _eventRepo.Setup(r => r.UpdateAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);

        await _service.RegisterMemberAsync(evt.Id, _paidMember.Id);

        Assert.That(evt.RegisteredMembers, Contains.Item(_paidMember.Id));
    }

    [Test]
    public void RegisterMemberAsync_AlreadyRegistered_Throws()
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            MaxParticipants = 10,
            RegistrationDeadline = DateTime.UtcNow.AddDays(5),
            RegisteredMembers = new List<Guid> { _paidMember.Id }
        };
        _eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterMemberAsync(evt.Id, _paidMember.Id));
    }

    [Test]
    public void RegisterMemberAsync_EventFull_Throws()
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            MaxParticipants = 1,
            RegistrationDeadline = DateTime.UtcNow.AddDays(5),
            RegisteredMembers = new List<Guid> { Guid.NewGuid() }
        };
        _eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterMemberAsync(evt.Id, _paidMember.Id));
    }

    [Test]
    public void RegisterMemberAsync_DeadlinePassed_Throws()
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            MaxParticipants = 10,
            RegistrationDeadline = DateTime.UtcNow.AddDays(-1)
        };
        _eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterMemberAsync(evt.Id, _paidMember.Id));
    }

    [Test]
    public void RegisterMemberAsync_EventNotFound_Throws()
    {
        _eventRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Event?)null);
        Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterMemberAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Test]
    public async Task UnregisterMemberAsync_RemovesMember()
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            RegisteredMembers = new List<Guid> { _paidMember.Id }
        };
        _eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
        _eventRepo.Setup(r => r.UpdateAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);

        await _service.UnregisterMemberAsync(evt.Id, _paidMember.Id);

        Assert.That(evt.RegisteredMembers, Does.Not.Contain(_paidMember.Id));
    }

    [Test]
    public async Task RecordResultsAsync_PaidMember_GetsPoints()
    {
        var evt = new Event { Id = Guid.NewGuid() };
        _eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
        _eventRepo.Setup(r => r.UpdateAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);
        _settingsService.Setup(s => s.GetPointsForPlacement(1)).ReturnsAsync(100);

        var results = new List<RecordResultDto>
        {
            new() { MemberId = _paidMember.Id, Placement = 1, Score = 54 }
        };

        await _service.RecordResultsAsync(evt.Id, results);

        Assert.That(evt.Results, Has.Count.EqualTo(1));
        Assert.That(evt.Results[0].PointsAwarded, Is.EqualTo(100));
    }

    [Test]
    public async Task RecordResultsAsync_UnpaidMember_GetsZeroPoints()
    {
        var evt = new Event { Id = Guid.NewGuid() };
        _eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
        _eventRepo.Setup(r => r.UpdateAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);
        _settingsService.Setup(s => s.GetPointsForPlacement(1)).ReturnsAsync(100);

        var results = new List<RecordResultDto>
        {
            new() { MemberId = _unpaidMember.Id, Placement = 1, Score = 52 }
        };

        await _service.RecordResultsAsync(evt.Id, results);

        Assert.That(evt.Results[0].PointsAwarded, Is.EqualTo(0));
    }

    [Test]
    public async Task CompleteEventAsync_SetsStatusToCompleted()
    {
        var evt = new Event { Id = Guid.NewGuid(), Status = EventStatus.Upcoming };
        _eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
        _eventRepo.Setup(r => r.UpdateAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);

        await _service.CompleteEventAsync(evt.Id);

        Assert.That(evt.Status, Is.EqualTo(EventStatus.Completed));
        _eventRepo.Verify(r => r.UpdateAsync(evt), Times.Once);
    }

    [Test]
    public async Task DeleteEventAsync_CallsRepository()
    {
        var id = Guid.NewGuid();
        _eventRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        await _service.DeleteEventAsync(id);

        _eventRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
