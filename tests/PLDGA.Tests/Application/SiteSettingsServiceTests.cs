using Moq;
using PLDGA.Application.Services;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Tests.Application;

[TestFixture]
public class SiteSettingsServiceTests
{
    private Mock<ISiteSettingsRepository> _settingsRepo = null!;
    private SiteSettingsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _settingsRepo = new Mock<ISiteSettingsRepository>();
        _service = new SiteSettingsService(_settingsRepo.Object);
    }

    [Test]
    public async Task GetSettingsAsync_ReturnsSettings()
    {
        var settings = new SiteSettings { AnnualDuesAmount = 30m, SiteTitle = "PLDGA" };
        _settingsRepo.Setup(r => r.GetAsync()).ReturnsAsync(settings);

        var result = await _service.GetSettingsAsync();

        Assert.That(result.AnnualDuesAmount, Is.EqualTo(30m));
        Assert.That(result.SiteTitle, Is.EqualTo("PLDGA"));
    }

    [Test]
    public async Task SaveSettingsAsync_CallsRepository()
    {
        var settings = new SiteSettings { AnnualDuesAmount = 50m };
        _settingsRepo.Setup(r => r.SaveAsync(settings)).Returns(Task.CompletedTask);

        await _service.SaveSettingsAsync(settings);

        _settingsRepo.Verify(r => r.SaveAsync(settings), Times.Once);
    }

    [Test]
    public async Task GetPointsForPlacement_ValidPlacement_ReturnsPoints()
    {
        var settings = new SiteSettings();
        _settingsRepo.Setup(r => r.GetAsync()).ReturnsAsync(settings);

        var result = await _service.GetPointsForPlacement(1);
        Assert.That(result, Is.EqualTo(100));

        result = await _service.GetPointsForPlacement(10);
        Assert.That(result, Is.EqualTo(51));
    }

    [Test]
    public async Task GetPointsForPlacement_InvalidPlacement_ReturnsZero()
    {
        var settings = new SiteSettings();
        _settingsRepo.Setup(r => r.GetAsync()).ReturnsAsync(settings);

        var result = await _service.GetPointsForPlacement(99);
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task GetPointsForPlacement_BoundaryValues()
    {
        var settings = new SiteSettings();
        _settingsRepo.Setup(r => r.GetAsync()).ReturnsAsync(settings);

        Assert.That(await _service.GetPointsForPlacement(1), Is.EqualTo(100));
        Assert.That(await _service.GetPointsForPlacement(20), Is.EqualTo(31));
        Assert.That(await _service.GetPointsForPlacement(0), Is.EqualTo(0));
        Assert.That(await _service.GetPointsForPlacement(21), Is.EqualTo(0));
    }
}
