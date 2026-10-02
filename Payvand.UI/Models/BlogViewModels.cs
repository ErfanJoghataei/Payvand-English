using Payvand.DAL.Entities;

namespace Payvand.UI.Models;

public sealed class BlogIndexViewModel : LinkViewModel
{
    public IReadOnlyList<Article> Articles { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
    public string? SelectedCategory { get; init; }
    public string? SearchQuery { get; init; }
    public Article? FeaturedArticle { get; init; }
}

public sealed class BlogDetailsViewModel : LinkViewModel
{
    public required Article Article { get; init; }
    public IReadOnlyList<Article> RelatedArticles { get; init; } = [];
    public IReadOnlyList<ArticleContentBlock> ContentBlocks { get; init; } = [];
}

public sealed record ArticleContentBlock(string Type, string Text);
