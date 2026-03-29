using PLDGA.Domain.Entities;

namespace PLDGA.Application.DTOs;

public class NewsArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid AuthorMemberId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public ArticleStatus Status { get; set; }
    public List<string> ImagePaths { get; set; } = new();
    public bool IsFeatured { get; set; }
}

public class CreateNewsArticleDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
}
