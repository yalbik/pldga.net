using PLDGA.Domain.Entities;
using PLDGA.Infrastructure.Persistence;

namespace PLDGA.Tests.Infrastructure;

[TestFixture]
public class JsonSiteSettingsRepositoryTests
{
    private string _testDir = null!;
    private JsonSiteSettingsRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "pldga_tests_" + Guid.NewGuid().ToString("N"));
        _repo = new JsonSiteSettingsRepository(_testDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Test]
    public async Task GetAsync_NoFile_ReturnsDefaults()
    {
        var result = await _repo.GetAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.AnnualDuesAmount, Is.EqualTo(30m));
        Assert.That(result.SiteTitle, Is.EqualTo("Parkland Disc Golf Association"));
    }

    [Test]
    public async Task SaveAndGetAsync_RoundTrip()
    {
        var settings = new SiteSettings { AnnualDuesAmount = 50m, SiteTitle = "Custom Title" };
        await _repo.SaveAsync(settings);

        var result = await _repo.GetAsync();

        Assert.That(result.AnnualDuesAmount, Is.EqualTo(50m));
        Assert.That(result.SiteTitle, Is.EqualTo("Custom Title"));
    }

    [Test]
    public async Task SaveAsync_Overwrite_PersistsLatest()
    {
        await _repo.SaveAsync(new SiteSettings { AnnualDuesAmount = 25m });
        await _repo.SaveAsync(new SiteSettings { AnnualDuesAmount = 75m });

        var result = await _repo.GetAsync();
        Assert.That(result.AnnualDuesAmount, Is.EqualTo(75m));
    }

    [Test]
    public async Task SaveAsync_PreservesPlacementPoints()
    {
        var settings = new SiteSettings();
        settings.PlacementPoints[1] = 200;
        await _repo.SaveAsync(settings);

        var result = await _repo.GetAsync();
        Assert.That(result.PlacementPoints[1], Is.EqualTo(200));
    }
}
