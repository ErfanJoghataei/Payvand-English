namespace Payvand.DAL.Entities;

public sealed class Article
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string Excerpt { get; set; }
    public required string Content { get; set; }
    public required string Category { get; set; }
    public required string CoverTheme { get; set; }
    public string AuthorName { get; set; } = "تیم پیوند";
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
}
