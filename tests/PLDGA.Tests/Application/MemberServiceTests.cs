using Moq;
using PLDGA.Application.DTOs;
using PLDGA.Application.Services;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Tests.Application;

[TestFixture]
public class MemberServiceTests
{
    private Mock<IMemberRepository> _memberRepo = null!;
    private MemberService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepo = new Mock<IMemberRepository>();
        _service = new MemberService(_memberRepo.Object);
    }

    [Test]
    public async Task GetAllMembersAsync_ReturnsMappedDtos()
    {
        var members = new List<Member>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@test.com", IsPaid = true },
            new() { FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", IsPaid = false }
        };
        _memberRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(members);

        var result = (await _service.GetAllMembersAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].FirstName, Is.EqualTo("John"));
        Assert.That(result[0].IsPaid, Is.True);
        Assert.That(result[1].FirstName, Is.EqualTo("Jane"));
    }

    [Test]
    public async Task GetMemberByIdAsync_Exists_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var member = new Member { Id = id, FirstName = "John", LastName = "Doe" };
        _memberRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(member);

        var result = await _service.GetMemberByIdAsync(id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(id));
        Assert.That(result.FirstName, Is.EqualTo("John"));
    }

    [Test]
    public async Task GetMemberByIdAsync_NotExists_ReturnsNull()
    {
        _memberRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Member?)null);

        var result = await _service.GetMemberByIdAsync(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateMemberAsync_CreatesAndReturnsDto()
    {
        var dto = new CreateMemberDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Phone = "555-1234",
            IsPaid = true,
            IsAdmin = false
        };

        _memberRepo.Setup(r => r.AddAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        var result = await _service.CreateMemberAsync(dto);

        Assert.That(result.FirstName, Is.EqualTo("John"));
        Assert.That(result.LastName, Is.EqualTo("Doe"));
        Assert.That(result.IsPaid, Is.True);
        Assert.That(result.PaymentDate, Is.Not.Null);
        _memberRepo.Verify(r => r.AddAsync(It.Is<Member>(m => m.FirstName == "John")), Times.Once);
    }

    [Test]
    public async Task CreateMemberAsync_Unpaid_NoPaymentDate()
    {
        var dto = new CreateMemberDto { FirstName = "Jane", LastName = "Doe", IsPaid = false };
        _memberRepo.Setup(r => r.AddAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        var result = await _service.CreateMemberAsync(dto);

        Assert.That(result.PaymentDate, Is.Null);
    }

    [Test]
    public async Task UpdateMemberAsync_ExistingMember_Updates()
    {
        var id = Guid.NewGuid();
        var member = new Member { Id = id, FirstName = "Old", LastName = "Name" };
        _memberRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(member);
        _memberRepo.Setup(r => r.UpdateAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        var dto = new UpdateMemberDto { Id = id, FirstName = "New", LastName = "Name", Email = "new@test.com" };
        await _service.UpdateMemberAsync(dto);

        _memberRepo.Verify(r => r.UpdateAsync(It.Is<Member>(m => m.FirstName == "New")), Times.Once);
    }

    [Test]
    public void UpdateMemberAsync_NotExists_Throws()
    {
        _memberRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Member?)null);
        var dto = new UpdateMemberDto { Id = Guid.NewGuid() };

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateMemberAsync(dto));
    }

    [Test]
    public async Task DeleteMemberAsync_CallsRepository()
    {
        var id = Guid.NewGuid();
        _memberRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        await _service.DeleteMemberAsync(id);

        _memberRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Test]
    public async Task SetPaymentStatusAsync_Paid_SetsDateAndStatus()
    {
        var id = Guid.NewGuid();
        var member = new Member { Id = id, IsPaid = false, PaymentDate = null };
        _memberRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(member);
        _memberRepo.Setup(r => r.UpdateAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        await _service.SetPaymentStatusAsync(id, true);

        _memberRepo.Verify(r => r.UpdateAsync(It.Is<Member>(m => m.IsPaid && m.PaymentDate != null)), Times.Once);
    }

    [Test]
    public async Task SetPaymentStatusAsync_Unpaid_ClearsDate()
    {
        var id = Guid.NewGuid();
        var member = new Member { Id = id, IsPaid = true, PaymentDate = DateTime.UtcNow };
        _memberRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(member);
        _memberRepo.Setup(r => r.UpdateAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        await _service.SetPaymentStatusAsync(id, false);

        _memberRepo.Verify(r => r.UpdateAsync(It.Is<Member>(m => !m.IsPaid && m.PaymentDate == null)), Times.Once);
    }

    [Test]
    public void SetPaymentStatusAsync_NotExists_Throws()
    {
        _memberRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Member?)null);
        Assert.ThrowsAsync<InvalidOperationException>(() => _service.SetPaymentStatusAsync(Guid.NewGuid(), true));
    }

    [Test]
    public async Task BulkSetPaymentStatusAsync_SetsAllMembers()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        foreach (var id in ids)
        {
            var member = new Member { Id = id, IsPaid = false };
            _memberRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(member);
        }
        _memberRepo.Setup(r => r.UpdateAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        await _service.BulkSetPaymentStatusAsync(ids, true);

        _memberRepo.Verify(r => r.UpdateAsync(It.IsAny<Member>()), Times.Exactly(2));
    }

    [Test]
    public async Task GetByPaymentStatusAsync_CorrectFilter()
    {
        var paidMembers = new List<Member> { new() { FirstName = "Paid", IsPaid = true } };
        _memberRepo.Setup(r => r.GetByPaymentStatusAsync(true)).ReturnsAsync(paidMembers);

        var result = (await _service.GetByPaymentStatusAsync(true)).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].FirstName, Is.EqualTo("Paid"));
    }

    [Test]
    public void MapToDto_CorrectlyMaps()
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Phone = "555-0000",
            IsPaid = true,
            IsAdmin = true,
            CurrentSeasonPoints = 250,
            PaymentDate = new DateTime(2025, 1, 15)
        };

        var dto = MemberService.MapToDto(member);

        Assert.That(dto.Id, Is.EqualTo(member.Id));
        Assert.That(dto.FirstName, Is.EqualTo("Test"));
        Assert.That(dto.LastName, Is.EqualTo("User"));
        Assert.That(dto.Email, Is.EqualTo("test@test.com"));
        Assert.That(dto.CurrentSeasonPoints, Is.EqualTo(250));
        Assert.That(dto.IsPaid, Is.True);
        Assert.That(dto.IsAdmin, Is.True);
    }
}
