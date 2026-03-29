using PLDGA.Domain.Entities;

namespace PLDGA.Domain.Interfaces;

public interface INewsArticleRepository
{
    Task<IEnumerable<NewsArticle>> GetAllAsync();
    Task<NewsArticle?> GetByIdAsync(Guid id);
    Task<IEnumerable<NewsArticle>> GetByStatusAsync(ArticleStatus status);
    Task<IEnumerable<NewsArticle>> GetByAuthorAsync(Guid authorMemberId);
    Task AddAsync(NewsArticle article);
    Task UpdateAsync(NewsArticle article);
    Task DeleteAsync(Guid id);
}
