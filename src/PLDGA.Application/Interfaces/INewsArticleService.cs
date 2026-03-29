using PLDGA.Application.DTOs;

namespace PLDGA.Application.Interfaces;

public interface INewsArticleService
{
    Task<IEnumerable<NewsArticleDto>> GetAllArticlesAsync();
    Task<NewsArticleDto?> GetArticleByIdAsync(Guid id);
    Task<IEnumerable<NewsArticleDto>> GetApprovedArticlesAsync();
    Task<IEnumerable<NewsArticleDto>> GetPendingArticlesAsync();
    Task<NewsArticleDto> CreateArticleAsync(CreateNewsArticleDto dto, Guid authorMemberId);
    Task UpdateArticleAsync(NewsArticleDto dto);
    Task ApproveArticleAsync(Guid id);
    Task RejectArticleAsync(Guid id);
    Task DeleteArticleAsync(Guid id);
    Task AddImageAsync(Guid articleId, string imagePath);
}
