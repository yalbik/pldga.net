using PLDGA.Domain.Entities;
using PLDGA.Infrastructure.Persistence;

namespace PLDGA.Tests.Infrastructure;

[TestFixture]
public class JsonFileStoreTests
{
    private string _testDir = null!;

    [SetUp]
    public void SetUp()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "pldga_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Test]
    public async Task ReadAllAsync_FileNotExists_ReturnsEmptyList()
    {
        var store = new JsonFileStore<Member>(_testDir, "members.json");
        var result = await store.ReadAllAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task WriteAndRead_RoundTrips()
    {
        var store = new JsonFileStore<Member>(_testDir, "members.json");
        var members = new List<Member>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new() { FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        };

        await store.WriteAllAsync(members);
        var result = await store.ReadAllAsync();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].FirstName, Is.EqualTo("John"));
        Assert.That(result[1].FirstName, Is.EqualTo("Jane"));
    }

    [Test]
    public async Task WriteAllAsync_OverwritesPreviousData()
    {
        var store = new JsonFileStore<Member>(_testDir, "members.json");

        await store.WriteAllAsync(new List<Member> { new() { FirstName = "First" } });
        await store.WriteAllAsync(new List<Member> { new() { FirstName = "Second" } });

        var result = await store.ReadAllAsync();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].FirstName, Is.EqualTo("Second"));
    }

    [Test]
    public async Task WriteAllAsync_CreatesFile()
    {
        var store = new JsonFileStore<Member>(_testDir, "test.json");
        await store.WriteAllAsync(new List<Member> { new() { FirstName = "Test" } });

        Assert.That(File.Exists(Path.Combine(_testDir, "test.json")), Is.True);
    }

    [Test]
    public async Task WriteAllAsync_AtomicWrite_NoTempFileLeft()
    {
        var store = new JsonFileStore<Member>(_testDir, "test.json");
        await store.WriteAllAsync(new List<Member> { new() { FirstName = "Test" } });

        Assert.That(File.Exists(Path.Combine(_testDir, "test.json.tmp")), Is.False);
    }
}

[TestFixture]
public class JsonSingleStoreTests
{
    private string _testDir = null!;

    [SetUp]
    public void SetUp()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "pldga_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Test]
    public async Task ReadAsync_FileNotExists_ReturnsNewInstance()
    {
        var store = new JsonSingleStore<SiteSettings>(_testDir, "settings.json");
        var result = await store.ReadAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.AnnualDuesAmount, Is.EqualTo(30m));
    }

    [Test]
    public async Task WriteAndRead_RoundTrips()
    {
        var store = new JsonSingleStore<SiteSettings>(_testDir, "settings.json");
        var settings = new SiteSettings { AnnualDuesAmount = 50m, SiteTitle = "Test PLDGA" };

        await store.WriteAsync(settings);
        var result = await store.ReadAsync();

        Assert.That(result.AnnualDuesAmount, Is.EqualTo(50m));
        Assert.That(result.SiteTitle, Is.EqualTo("Test PLDGA"));
    }

    [Test]
    public async Task WriteAsync_OverwritesPrevious()
    {
        var store = new JsonSingleStore<SiteSettings>(_testDir, "settings.json");

        await store.WriteAsync(new SiteSettings { AnnualDuesAmount = 25m });
        await store.WriteAsync(new SiteSettings { AnnualDuesAmount = 50m });

        var result = await store.ReadAsync();
        Assert.That(result.AnnualDuesAmount, Is.EqualTo(50m));
    }
}
