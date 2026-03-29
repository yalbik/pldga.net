using Moq;
using PLDGA.Application.DTOs;
using PLDGA.Application.Services;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Tests.Application;

[TestFixture]
public class NewsArticleServiceTests
{
    private Mock<INewsArticleRepository> _articleRepo = null!;
    private Mock<IMemberRepository> _memberRepo = null!;
    private NewsArticleService _service = null!;
    private Member _author = null!;

    [SetUp]
    public void SetUp()
    {
        _articleRepo = new Mock<INewsArticleRepository>();
        _memberRepo = new Mock<IMemberRepository>();
        _service = new NewsArticleService(_articleRepo.Object, _memberRepo.Object);

        _author = new Member { Id = Guid.NewGuid(), FirstName = "Author", LastName = "Person" };
        _memberRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Member> { _author });
    }

    [Test]
    public async Task GetAllArticlesAsync_ReturnsSortedByDateDesc()
    {
        var articles = new List<NewsArticle>
        {
            new() { Title = "Old", CreatedDate = DateTime.UtcNow.AddDays(-10), AuthorMemberId = _author.Id },
            new() { Title = "New", CreatedDate = DateTime.UtcNow, AuthorMemberId = _author.Id }
        };
        _articleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(articles);

        var result = (await _service.GetAllArticlesAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Title, Is.EqualTo("New"));
    }

    [Test]
    public async Task GetArticleByIdAsync_Exists_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var article = new NewsArticle { Id = id, Title = "Test", AuthorMemberId = _author.Id };
        _articleRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(article);

        var result = await _service.GetArticleByIdAsync(id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("Test"));
        Assert.That(result.AuthorName, Is.EqualTo("Author Person"));
    }

    [Test]
    public async Task GetArticleByIdAsync_NotExists_ReturnsNull()
    {
        _articleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NewsArticle?)null);
        var result = await _service.GetArticleByIdAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetApprovedArticlesAsync_FiltersCorrectly()
    {
        var articles = new List<NewsArticle>
        {
            new() { Title = "Approved", Status = ArticleStatus.Approved, AuthorMemberId = _author.Id }
        };
        _articleRepo.Setup(r => r.GetByStatusAsync(ArticleStatus.Approved)).ReturnsAsync(articles);

        var result = (await _service.GetApprovedArticlesAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Status, Is.EqualTo(ArticleStatus.Approved));
    }

    [Test]
    public async Task GetPendingArticlesAsync_FiltersCorrectly()
    {
        var articles = new List<NewsArticle>
        {
            new() { Title = "Pending", Status = ArticleStatus.Pending, AuthorMemberId = _author.Id }
        };
        _articleRepo.Setup(r => r.GetByStatusAsync(ArticleStatus.Pending)).ReturnsAsync(articles);

        var result = (await _service.GetPendingArticlesAsync()).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task CreateArticleAsync_CreatesPendingArticle()
    {
        var dto = new CreateNewsArticleDto { Title = "New Article", Content = "Content here", IsFeatured = true };
        _articleRepo.Setup(r => r.AddAsync(It.IsAny<NewsArticle>())).Returns(Task.CompletedTask);

        var result = await _service.CreateArticleAsync(dto, _author.Id);

        Assert.That(result.Title, Is.EqualTo("New Article"));
        Assert.That(result.Status, Is.EqualTo(ArticleStatus.Pending));
        Assert.That(result.IsFeatured, Is.True);
        _articleRepo.Verify(r => r.AddAsync(It.Is<NewsArticle>(a => a.Status == ArticleStatus.Pending)), Times.Once);
    }

    [Test]
    public async Task ApproveArticleAsync_SetsApprovedStatus()
    {
        var article = new NewsArticle { Id = Guid.NewGuid(), Status = ArticleStatus.Pending };
        _articleRepo.Setup(r => r.GetByIdAsync(article.Id)).ReturnsAsync(article);
        _articleRepo.Setup(r => r.UpdateAsync(It.IsAny<NewsArticle>())).Returns(Task.CompletedTask);

        await _service.ApproveArticleAsync(article.Id);

        Assert.That(article.Status, Is.EqualTo(ArticleStatus.Approved));
        Assert.That(article.LastModifiedDate, Is.Not.Null);
    }

    [Test]
    public async Task RejectArticleAsync_SetsRejectedStatus()
    {
        var article = new NewsArticle { Id = Guid.NewGuid(), Status = ArticleStatus.Pending };
        _articleRepo.Setup(r => r.GetByIdAsync(article.Id)).ReturnsAsync(article);
        _articleRepo.Setup(r => r.UpdateAsync(It.IsAny<NewsArticle>())).Returns(Task.CompletedTask);

        await _service.RejectArticleAsync(article.Id);

        Assert.That(article.Status, Is.EqualTo(ArticleStatus.Rejected));
    }

    [Test]
    public void ApproveArticleAsync_NotFound_Throws()
    {
        _articleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NewsArticle?)null);
        Assert.ThrowsAsync<InvalidOperationException>(() => _service.ApproveArticleAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task DeleteArticleAsync_CallsRepository()
    {
        var id = Guid.NewGuid();
        _articleRepo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        await _service.DeleteArticleAsync(id);

        _articleRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Test]
    public async Task AddImageAsync_AddsPathToArticle()
    {
        var article = new NewsArticle { Id = Guid.NewGuid() };
        _articleRepo.Setup(r => r.GetByIdAsync(article.Id)).ReturnsAsync(article);
        _articleRepo.Setup(r => r.UpdateAsync(It.IsAny<NewsArticle>())).Returns(Task.CompletedTask);

        await _service.AddImageAsync(article.Id, "/images/test.jpg");

        Assert.That(article.ImagePaths, Contains.Item("/images/test.jpg"));
        Assert.That(article.LastModifiedDate, Is.Not.Null);
    }

    [Test]
    public void AddImageAsync_NotFound_Throws()
    {
        _articleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NewsArticle?)null);
        Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddImageAsync(Guid.NewGuid(), "/test.jpg"));
    }

    [Test]
    public async Task UpdateArticleAsync_UpdatesFields()
    {
        var article = new NewsArticle { Id = Guid.NewGuid(), Title = "Old Title", Content = "Old" };
        _articleRepo.Setup(r => r.GetByIdAsync(article.Id)).ReturnsAsync(article);
        _articleRepo.Setup(r => r.UpdateAsync(It.IsAny<NewsArticle>())).Returns(Task.CompletedTask);

        var dto = new NewsArticleDto { Id = article.Id, Title = "New Title", Content = "New", IsFeatured = true };
        await _service.UpdateArticleAsync(dto);

        Assert.That(article.Title, Is.EqualTo("New Title"));
        Assert.That(article.Content, Is.EqualTo("New"));
        Assert.That(article.IsFeatured, Is.True);
    }
}
