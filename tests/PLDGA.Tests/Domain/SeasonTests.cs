using PLDGA.Domain.Entities;

namespace PLDGA.Tests.Domain;

[TestFixture]
public class SeasonTests
{
    [Test]
    public void NewSeason_HasDefaultValues()
    {
        var season = new Season();

        Assert.That(season.Year, Is.EqualTo(0));
        Assert.That(season.IsCurrent, Is.False);
        Assert.That(season.EventIds, Is.Empty);
    }

    [Test]
    public void Season_StoresProperties()
    {
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 12, 31);
        var season = new Season
        {
            Year = 2025,
            StartDate = start,
            EndDate = end,
            IsCurrent = true
        };

        Assert.That(season.Year, Is.EqualTo(2025));
        Assert.That(season.StartDate, Is.EqualTo(start));
        Assert.That(season.EndDate, Is.EqualTo(end));
        Assert.That(season.IsCurrent, Is.True);
    }
}
