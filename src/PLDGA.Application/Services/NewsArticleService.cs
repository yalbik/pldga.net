using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Application.Services;

public class NewsArticleService : INewsArticleService
{
    private readonly INewsArticleRepository _articleRepository;
    private readonly IMemberRepository _memberRepository;

    public NewsArticleService(INewsArticleRepository articleRepository, IMemberRepository memberRepository)
    {
        _articleRepository = articleRepository;
        _memberRepository = memberRepository;
    }

    public async Task<IEnumerable<NewsArticleDto>> GetAllArticlesAsync()
    {
        var articles = await _articleRepository.GetAllAsync();
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return articles.Select(a => MapToDto(a, members)).OrderByDescending(a => a.CreatedDate);
    }

    public async Task<NewsArticleDto?> GetArticleByIdAsync(Guid id)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null) return null;
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return MapToDto(article, members);
    }

    public async Task<IEnumerable<NewsArticleDto>> GetApprovedArticlesAsync()
    {
        var articles = await _articleRepository.GetByStatusAsync(ArticleStatus.Approved);
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return articles.Select(a => MapToDto(a, members)).OrderByDescending(a => a.CreatedDate);
    }

    public async Task<IEnumerable<NewsArticleDto>> GetPendingArticlesAsync()
    {
        var articles = await _articleRepository.GetByStatusAsync(ArticleStatus.Pending);
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return articles.Select(a => MapToDto(a, members)).OrderByDescending(a => a.CreatedDate);
    }

    public async Task<NewsArticleDto> CreateArticleAsync(CreateNewsArticleDto dto, Guid authorMemberId)
    {
        var article = new NewsArticle
        {
            Title = dto.Title,
            Content = dto.Content,
            AuthorMemberId = authorMemberId,
            IsFeatured = dto.IsFeatured,
            Status = ArticleStatus.Pending
        };

        await _articleRepository.AddAsync(article);
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return MapToDto(article, members);
    }

    public async Task UpdateArticleAsync(NewsArticleDto dto)
    {
        var article = await _articleRepository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException("Article not found.");

        article.Title = dto.Title;
        article.Content = dto.Content;
        article.IsFeatured = dto.IsFeatured;
        article.LastModifiedDate = DateTime.UtcNow;

        await _articleRepository.UpdateAsync(article);
    }

    public async Task ApproveArticleAsync(Guid id)
    {
        var article = await _articleRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Article not found.");

        article.Status = ArticleStatus.Approved;
        article.LastModifiedDate = DateTime.UtcNow;
        await _articleRepository.UpdateAsync(article);
    }

    public async Task RejectArticleAsync(Guid id)
    {
        var article = await _articleRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Article not found.");

        article.Status = ArticleStatus.Rejected;
        article.LastModifiedDate = DateTime.UtcNow;
        await _articleRepository.UpdateAsync(article);
    }

    public async Task DeleteArticleAsync(Guid id)
    {
        await _articleRepository.DeleteAsync(id);
    }

    public async Task AddImageAsync(Guid articleId, string imagePath)
    {
        var article = await _articleRepository.GetByIdAsync(articleId)
            ?? throw new InvalidOperationException("Article not found.");

        article.ImagePaths.Add(imagePath);
        article.LastModifiedDate = DateTime.UtcNow;
        await _articleRepository.UpdateAsync(article);
    }

    private static NewsArticleDto MapToDto(NewsArticle a, Dictionary<Guid, Member> members) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Content = a.Content,
        AuthorMemberId = a.AuthorMemberId,
        AuthorName = members.TryGetValue(a.AuthorMemberId, out var m) ? m.FullName : "Unknown",
        CreatedDate = a.CreatedDate,
        LastModifiedDate = a.LastModifiedDate,
        Status = a.Status,
        ImagePaths = a.ImagePaths,
        IsFeatured = a.IsFeatured
    };
}
