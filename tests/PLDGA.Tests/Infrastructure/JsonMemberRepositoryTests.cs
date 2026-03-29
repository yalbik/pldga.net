using PLDGA.Domain.Entities;
using PLDGA.Infrastructure.Persistence;

namespace PLDGA.Tests.Infrastructure;

[TestFixture]
public class JsonMemberRepositoryTests
{
    private string _testDir = null!;
    private JsonMemberRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "pldga_tests_" + Guid.NewGuid().ToString("N"));
        _repo = new JsonMemberRepository(_testDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Test]
    public async Task GetAllAsync_Empty_ReturnsEmpty()
    {
        var result = await _repo.GetAllAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task AddAndGetById_RoundTrip()
    {
        var member = new Member { FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        await _repo.AddAsync(member);

        var result = await _repo.GetByIdAsync(member.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.FirstName, Is.EqualTo("John"));
        Assert.That(result.Email, Is.EqualTo("john@test.com"));
    }

    [Test]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_UpdatesExistingMember()
    {
        var member = new Member { FirstName = "John", LastName = "Doe" };
        await _repo.AddAsync(member);

        member.FirstName = "Jane";
        await _repo.UpdateAsync(member);

        var result = await _repo.GetByIdAsync(member.Id);
        Assert.That(result!.FirstName, Is.EqualTo("Jane"));
    }

    [Test]
    public async Task DeleteAsync_RemovesMember()
    {
        var member = new Member { FirstName = "John", LastName = "Doe" };
        await _repo.AddAsync(member);
        await _repo.DeleteAsync(member.Id);

        var result = await _repo.GetByIdAsync(member.Id);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByPaymentStatusAsync_FiltersCorrectly()
    {
        await _repo.AddAsync(new Member { FirstName = "Paid", IsPaid = true });
        await _repo.AddAsync(new Member { FirstName = "Unpaid", IsPaid = false });

        var paid = (await _repo.GetByPaymentStatusAsync(true)).ToList();
        var unpaid = (await _repo.GetByPaymentStatusAsync(false)).ToList();

        Assert.That(paid, Has.Count.EqualTo(1));
        Assert.That(paid[0].FirstName, Is.EqualTo("Paid"));
        Assert.That(unpaid, Has.Count.EqualTo(1));
        Assert.That(unpaid[0].FirstName, Is.EqualTo("Unpaid"));
    }

    [Test]
    public async Task GetByUserIdAsync_FindsByUserId()
    {
        var member = new Member { FirstName = "John", UserId = "user-123" };
        await _repo.AddAsync(member);

        var result = await _repo.GetByUserIdAsync("user-123");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.FirstName, Is.EqualTo("John"));
    }

    [Test]
    public async Task GetByUserIdAsync_NotFound_ReturnsNull()
    {
        var result = await _repo.GetByUserIdAsync("nonexistent");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task AddMultiple_AllPersisted()
    {
        for (int i = 0; i < 5; i++)
            await _repo.AddAsync(new Member { FirstName = $"Member{i}" });

        var all = (await _repo.GetAllAsync()).ToList();
        Assert.That(all, Has.Count.EqualTo(5));
    }
}
