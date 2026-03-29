using Moq;
using PLDGA.Application.DTOs;
using PLDGA.Application.Services;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Tests.Application;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepo = null!;
    private Mock<IMemberRepository> _memberRepo = null!;
    private AuthService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepo = new Mock<IUserRepository>();
        _memberRepo = new Mock<IMemberRepository>();
        _service = new AuthService(_userRepo.Object, _memberRepo.Object);
    }

    [Test]
    public void HashPassword_ProducesValidHash()
    {
        var hash = _service.HashPassword("TestPassword123");

        Assert.That(hash, Is.Not.Null.And.Not.Empty);
        Assert.That(hash, Does.Contain("."));
        var parts = hash.Split('.');
        Assert.That(parts, Has.Length.EqualTo(2));
    }

    [Test]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _service.HashPassword("TestPassword123");
        Assert.That(_service.VerifyPassword("TestPassword123", hash), Is.True);
    }

    [Test]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _service.HashPassword("TestPassword123");
        Assert.That(_service.VerifyPassword("WrongPassword", hash), Is.False);
    }

    [Test]
    public void VerifyPassword_InvalidHash_ReturnsFalse()
    {
        Assert.That(_service.VerifyPassword("test", "invalidhash"), Is.False);
    }

    [Test]
    public void HashPassword_DifferentSaltsPerCall()
    {
        var hash1 = _service.HashPassword("SamePassword");
        var hash2 = _service.HashPassword("SamePassword");
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var hash = _service.HashPassword("Admin123!");
        var user = new UserAccount
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = hash,
            IsAdmin = true,
            MemberId = Guid.NewGuid()
        };
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<UserAccount>())).Returns(Task.CompletedTask);

        var result = await _service.LoginAsync(new LoginDto { Username = "admin", Password = "Admin123!" });

        Assert.That(result.Success, Is.True);
        Assert.That(result.Username, Is.EqualTo("admin"));
        Assert.That(result.IsAdmin, Is.True);
        Assert.That(result.UserId, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task LoginAsync_WrongPassword_ReturnsFail()
    {
        var hash = _service.HashPassword("Real123!");
        var user = new UserAccount { Username = "admin", PasswordHash = hash };
        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

        var result = await _service.LoginAsync(new LoginDto { Username = "admin", Password = "Wrong!" });

        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null);
    }

    [Test]
    public async Task LoginAsync_UserNotFound_ReturnsFail()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((UserAccount?)null);

        var result = await _service.LoginAsync(new LoginDto { Username = "nonexistent", Password = "pass" });

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task LoginAsync_UpdatesLastLoginDate()
    {
        var hash = _service.HashPassword("Test123!");
        var user = new UserAccount { Username = "user", PasswordHash = hash, LastLoginDate = null };
        _userRepo.Setup(r => r.GetByUsernameAsync("user")).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<UserAccount>())).Returns(Task.CompletedTask);

        await _service.LoginAsync(new LoginDto { Username = "user", Password = "Test123!" });

        Assert.That(user.LastLoginDate, Is.Not.Null);
        _userRepo.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task RegisterAsync_NewUser_ReturnsSuccess()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("newuser")).ReturnsAsync((UserAccount?)null);
        _memberRepo.Setup(r => r.AddAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);
        _memberRepo.Setup(r => r.UpdateAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<UserAccount>())).Returns(Task.CompletedTask);

        var dto = new CreateMemberDto
        {
            FirstName = "New",
            LastName = "User",
            Email = "new@test.com",
            Username = "newuser",
            Password = "Pass123!"
        };

        var result = await _service.RegisterAsync(dto);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Username, Is.EqualTo("newuser"));
        _memberRepo.Verify(r => r.AddAsync(It.IsAny<Member>()), Times.Once);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<UserAccount>()), Times.Once);
    }

    [Test]
    public async Task RegisterAsync_DuplicateUsername_ReturnsFail()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("existing")).ReturnsAsync(new UserAccount());

        var dto = new CreateMemberDto { Username = "existing", Password = "Pass123!" };
        var result = await _service.RegisterAsync(dto);

        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("already exists"));
    }

    [Test]
    public async Task ChangePasswordAsync_ValidCurrent_ReturnsTrue()
    {
        var hash = _service.HashPassword("Old123!");
        var user = new UserAccount { Id = Guid.NewGuid(), PasswordHash = hash };
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<UserAccount>())).Returns(Task.CompletedTask);

        var result = await _service.ChangePasswordAsync(user.Id, "Old123!", "New123!");

        Assert.That(result, Is.True);
        Assert.That(_service.VerifyPassword("New123!", user.PasswordHash), Is.True);
    }

    [Test]
    public async Task ChangePasswordAsync_WrongCurrent_ReturnsFalse()
    {
        var hash = _service.HashPassword("Real123!");
        var user = new UserAccount { Id = Guid.NewGuid(), PasswordHash = hash };
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _service.ChangePasswordAsync(user.Id, "Wrong!", "New123!");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ChangePasswordAsync_UserNotFound_ReturnsFalse()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserAccount?)null);

        var result = await _service.ChangePasswordAsync(Guid.NewGuid(), "old", "new");

        Assert.That(result, Is.False);
    }
}
