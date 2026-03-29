using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Infrastructure.Persistence;

public class JsonNewsArticleRepository : INewsArticleRepository
{
    private readonly JsonFileStore<NewsArticle> _store;

    public JsonNewsArticleRepository(string dataDirectory)
    {
        _store = new JsonFileStore<NewsArticle>(dataDirectory, "news_articles.json");
    }

    public async Task<IEnumerable<NewsArticle>> GetAllAsync()
    {
        return await _store.ReadAllAsync();
    }

    public async Task<NewsArticle?> GetByIdAsync(Guid id)
    {
        var articles = await _store.ReadAllAsync();
        return articles.FirstOrDefault(a => a.Id == id);
    }

    public async Task<IEnumerable<NewsArticle>> GetByStatusAsync(ArticleStatus status)
    {
        var articles = await _store.ReadAllAsync();
        return articles.Where(a => a.Status == status);
    }

    public async Task<IEnumerable<NewsArticle>> GetByAuthorAsync(Guid authorMemberId)
    {
        var articles = await _store.ReadAllAsync();
        return articles.Where(a => a.AuthorMemberId == authorMemberId);
    }

    public async Task AddAsync(NewsArticle article)
    {
        var articles = await _store.ReadAllAsync();
        articles.Add(article);
        await _store.WriteAllAsync(articles);
    }

    public async Task UpdateAsync(NewsArticle article)
    {
        var articles = await _store.ReadAllAsync();
        var index = articles.FindIndex(a => a.Id == article.Id);
        if (index >= 0)
        {
            articles[index] = article;
            await _store.WriteAllAsync(articles);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var articles = await _store.ReadAllAsync();
        articles.RemoveAll(a => a.Id == id);
        await _store.WriteAllAsync(articles);
    }
}
