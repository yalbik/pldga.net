using PLDGA.Domain.Entities;

namespace PLDGA.Tests.Domain;

[TestFixture]
public class EventTests
{
    [Test]
    public void NewEvent_HasDefaultValues()
    {
        var evt = new Event();

        Assert.That(evt.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(evt.Name, Is.EqualTo(string.Empty));
        Assert.That(evt.MaxParticipants, Is.EqualTo(72));
        Assert.That(evt.Status, Is.EqualTo(EventStatus.Upcoming));
        Assert.That(evt.RegisteredMembers, Is.Empty);
        Assert.That(evt.Results, Is.Empty);
    }

    [Test]
    public void EventResult_StoresProperties()
    {
        var memberId = Guid.NewGuid();
        var result = new EventResult
        {
            MemberId = memberId,
            Placement = 1,
            PointsAwarded = 100,
            Score = 54
        };

        Assert.That(result.MemberId, Is.EqualTo(memberId));
        Assert.That(result.Placement, Is.EqualTo(1));
        Assert.That(result.PointsAwarded, Is.EqualTo(100));
        Assert.That(result.Score, Is.EqualTo(54));
    }

    [Test]
    public void EventStatus_HasAllValues()
    {
        Assert.That(Enum.GetValues<EventStatus>(), Has.Length.EqualTo(3));
        Assert.That(Enum.IsDefined(EventStatus.Upcoming), Is.True);
        Assert.That(Enum.IsDefined(EventStatus.Active), Is.True);
        Assert.That(Enum.IsDefined(EventStatus.Completed), Is.True);
    }
}
