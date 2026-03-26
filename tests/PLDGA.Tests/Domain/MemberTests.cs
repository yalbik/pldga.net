using PLDGA.Domain.Entities;

namespace PLDGA.Tests.Domain;

[TestFixture]
public class MemberTests
{
    [Test]
    public void NewMember_HasDefaultValues()
    {
        var member = new Member();

        Assert.That(member.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(member.FirstName, Is.EqualTo(string.Empty));
        Assert.That(member.LastName, Is.EqualTo(string.Empty));
        Assert.That(member.Email, Is.EqualTo(string.Empty));
        Assert.That(member.IsPaid, Is.False);
        Assert.That(member.IsAdmin, Is.False);
        Assert.That(member.CurrentSeasonPoints, Is.EqualTo(0));
    }

    [Test]
    public void FullName_CombinesFirstAndLast()
    {
        var member = new Member { FirstName = "John", LastName = "Doe" };
        Assert.That(member.FullName, Is.EqualTo("John Doe"));
    }

    [Test]
    public void PaymentStatusLabel_ReturnsCorrectLabel()
    {
        var paid = new Member { IsPaid = true };
        var unpaid = new Member { IsPaid = false };

        Assert.That(paid.PaymentStatusLabel, Is.EqualTo("Paid"));
        Assert.That(unpaid.PaymentStatusLabel, Is.EqualTo("Unpaid"));
    }

    [Test]
    public void TwoMembers_HaveDifferentIds()
    {
        var m1 = new Member();
        var m2 = new Member();
        Assert.That(m1.Id, Is.Not.EqualTo(m2.Id));
    }
}
