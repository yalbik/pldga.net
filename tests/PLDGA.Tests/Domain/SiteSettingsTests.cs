using PLDGA.Domain.Entities;

namespace PLDGA.Tests.Domain;

[TestFixture]
public class SiteSettingsTests
{
    [Test]
    public void NewSettings_HasCorrectDefaults()
    {
        var settings = new SiteSettings();

        Assert.That(settings.AnnualDuesAmount, Is.EqualTo(30.00m));
        Assert.That(settings.DefaultMaxParticipants, Is.EqualTo(72));
        Assert.That(settings.TopPlayersOnHomePage, Is.EqualTo(10));
        Assert.That(settings.MaxPollAnswers, Is.EqualTo(10));
        Assert.That(settings.DefaultPollDurationDays, Is.EqualTo(7));
        Assert.That(settings.MaxImageSizeMb, Is.EqualTo(5));
        Assert.That(settings.SiteTitle, Is.EqualTo("Parkland Disc Golf Association"));
        Assert.That(settings.PrimaryColor, Is.EqualTo("#4A8FC2"));
        Assert.That(settings.CurrentSeasonYear, Is.EqualTo(DateTime.UtcNow.Year));
        Assert.That(settings.MaintenanceMode, Is.False);
        Assert.That(settings.ShowRanks, Is.True);
        Assert.That(settings.ShowPoints, Is.True);
    }

    [Test]
    public void PlacementPoints_HasCorrectDefaults()
    {
        var settings = new SiteSettings();

        Assert.That(settings.PlacementPoints, Has.Count.EqualTo(20));
        Assert.That(settings.PlacementPoints[1], Is.EqualTo(100));
        Assert.That(settings.PlacementPoints[2], Is.EqualTo(90));
        Assert.That(settings.PlacementPoints[10], Is.EqualTo(51));
        Assert.That(settings.PlacementPoints[20], Is.EqualTo(31));
    }

    [Test]
    public void HomePageSections_HasCorrectDefaults()
    {
        var settings = new SiteSettings();

        Assert.That(settings.HomePageSections, Has.Count.EqualTo(4));
        Assert.That(settings.HomePageSections[0].Key, Is.EqualTo("top_players"));
        Assert.That(settings.HomePageSections[0].Enabled, Is.True);
        Assert.That(settings.HomePageSections[3].Key, Is.EqualTo("polls"));
    }

    [Test]
    public void HomePageSection_StoresProperties()
    {
        var section = new HomePageSection { Key = "test", Title = "Test", Enabled = false, Order = 5 };

        Assert.That(section.Key, Is.EqualTo("test"));
        Assert.That(section.Title, Is.EqualTo("Test"));
        Assert.That(section.Enabled, Is.False);
        Assert.That(section.Order, Is.EqualTo(5));
    }
}
