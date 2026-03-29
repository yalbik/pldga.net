using PLDGA.Domain.Entities;

namespace PLDGA.Tests.Domain;

[TestFixture]
public class NewsArticleTests
{
    [Test]
    public void NewArticle_HasDefaultValues()
    {
        var article = new NewsArticle();

        Assert.That(article.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(article.Status, Is.EqualTo(ArticleStatus.Pending));
        Assert.That(article.ImagePaths, Is.Empty);
        Assert.That(article.IsFeatured, Is.False);
        Assert.That(article.LastModifiedDate, Is.Null);
    }

    [Test]
    public void ArticleStatus_HasAllValues()
    {
        Assert.That(Enum.GetValues<ArticleStatus>(), Has.Length.EqualTo(3));
        Assert.That(Enum.IsDefined(ArticleStatus.Pending), Is.True);
        Assert.That(Enum.IsDefined(ArticleStatus.Approved), Is.True);
        Assert.That(Enum.IsDefined(ArticleStatus.Rejected), Is.True);
    }
}
