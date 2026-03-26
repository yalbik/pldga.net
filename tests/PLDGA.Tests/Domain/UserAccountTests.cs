using PLDGA.Domain.Entities;

namespace PLDGA.Tests.Domain;

[TestFixture]
public class UserAccountTests
{
    [Test]
    public void NewUser_HasDefaultValues()
    {
        var user = new UserAccount();

        Assert.That(user.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(user.Username, Is.EqualTo(string.Empty));
        Assert.That(user.PasswordHash, Is.EqualTo(string.Empty));
        Assert.That(user.IsAdmin, Is.False);
        Assert.That(user.LastLoginDate, Is.Null);
    }

    [Test]
    public void TwoUsers_HaveDifferentIds()
    {
        var u1 = new UserAccount();
        var u2 = new UserAccount();
        Assert.That(u1.Id, Is.Not.EqualTo(u2.Id));
    }
}
