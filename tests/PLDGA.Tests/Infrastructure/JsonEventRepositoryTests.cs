using PLDGA.Domain.Entities;
using PLDGA.Infrastructure.Persistence;

namespace PLDGA.Tests.Infrastructure;

[TestFixture]
public class JsonEventRepositoryTests
{
    private string _testDir = null!;
    private JsonEventRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "pldga_tests_" + Guid.NewGuid().ToString("N"));
        _repo = new JsonEventRepository(_testDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Test]
    public async Task AddAndGetById_RoundTrip()
    {
        var evt = new Event { Name = "Test Event", Date = DateTime.UtcNow, SeasonYear = 2025 };
        await _repo.AddAsync(evt);

        var result = await _repo.GetByIdAsync(evt.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Test Event"));
    }

    [Test]
    public async Task GetBySeasonAsync_FiltersCorrectly()
    {
        await _repo.AddAsync(new Event { Name = "2025 Event", SeasonYear = 2025 });
        await _repo.AddAsync(new Event { Name = "2024 Event", SeasonYear = 2024 });

        var result = (await _repo.GetBySeasonAsync(2025)).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("2025 Event"));
    }

    [Test]
    public async Task GetByStatusAsync_FiltersCorrectly()
    {
        await _repo.AddAsync(new Event { Name = "Upcoming", Status = EventStatus.Upcoming });
        await _repo.AddAsync(new Event { Name = "Completed", Status = EventStatus.Completed });

        var upcoming = (await _repo.GetByStatusAsync(EventStatus.Upcoming)).ToList();
        var completed = (await _repo.GetByStatusAsync(EventStatus.Completed)).ToList();

        Assert.That(upcoming, Has.Count.EqualTo(1));
        Assert.That(upcoming[0].Name, Is.EqualTo("Upcoming"));
        Assert.That(completed, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task UpdateAsync_PersistsResults()
    {
        var evt = new Event { Name = "Test" };
        await _repo.AddAsync(evt);

        evt.Results.Add(new EventResult { MemberId = Guid.NewGuid(), Placement = 1, PointsAwarded = 100 });
        await _repo.UpdateAsync(evt);

        var result = await _repo.GetByIdAsync(evt.Id);
        Assert.That(result!.Results, Has.Count.EqualTo(1));
        Assert.That(result.Results[0].PointsAwarded, Is.EqualTo(100));
    }

    [Test]
    public async Task DeleteAsync_RemovesEvent()
    {
        var evt = new Event { Name = "Delete me" };
        await _repo.AddAsync(evt);
        await _repo.DeleteAsync(evt.Id);

        var result = await _repo.GetByIdAsync(evt.Id);
        Assert.That(result, Is.Null);
    }
}
