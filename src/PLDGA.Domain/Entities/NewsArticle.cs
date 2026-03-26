namespace PLDGA.Domain.Entities;

public enum ArticleStatus
{
    Pending,
    Approved,
    Rejected
}

public class NewsArticle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid AuthorMemberId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedDate { get; set; }
    public ArticleStatus Status { get; set; } = ArticleStatus.Pending;
    public List<string> ImagePaths { get; set; } = new();
    public bool IsFeatured { get; set; }
}
