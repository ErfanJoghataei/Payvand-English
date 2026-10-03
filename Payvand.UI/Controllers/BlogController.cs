using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payvand.DAL.AppDbContext;
using Payvand.UI.Models;

namespace Payvand.UI.Controllers;

[Route("blog")]
public sealed class BlogController : Controller
{
    private static readonly string[] EnglishArticleSlugs =
    [
        "what-is-short-link",
        "qr-code-complete-guide",
        "analyze-link-clicks",
        "short-link-security-tips",
        "qr-code-marketing-ideas"
    ];

    private readonly PayvandDbContext db;

    public BlogController(PayvandDbContext db)
    {
        this.db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? category = null, string? q = null)
    {
        category = category?.Trim();
        q = q?.Trim();

        var query = db.Articles.AsNoTracking()
            .Where(article => EnglishArticleSlugs.Contains(article.Slug));
        var categories = await query
            .Select(c => c.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(c => c.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(c =>
                c.Title.Contains(q) ||
                c.Excerpt.Contains(q) ||
                (c.Keywords != null && c.Keywords.Contains(q)));
        }

        var articles = await query
            .OrderByDescending(c => c.IsFeatured)
            .ThenByDescending(c => c.PublishedAt)
            .ToListAsync();

        ViewData["Seo"] = new SeoMetadata
        {
            Title = "Payvand Journal | Short Links, QR Codes, and Digital Growth",
            Description = "Practical guides to short links, QR codes, analytics, security, and digital growth.",
            Keywords = "Payvand Journal, short links, QR codes, link analytics, digital marketing",
            CanonicalPath = "/blog"
        };

        return View(new BlogIndexViewModel
        {
            Articles = articles,
            Categories = categories,
            SelectedCategory = category,
            SearchQuery = q,
            FeaturedArticle = string.IsNullOrWhiteSpace(category) && string.IsNullOrWhiteSpace(q)
                ? articles.FirstOrDefault(c => c.IsFeatured)
                : null
        });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        if (!EnglishArticleSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        var article = await db.Articles.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (article == null)
        {
            return NotFound();
        }

        var related = await db.Articles.AsNoTracking()
            .Where(c => c.Id != article.Id && c.Category == article.Category && EnglishArticleSlugs.Contains(c.Slug))
            .OrderByDescending(c => c.PublishedAt)
            .Take(3)
            .ToListAsync();

        ViewData["Seo"] = new SeoMetadata
        {
            Title = article.MetaTitle ?? $"{article.Title} | Payvand Journal",
            Description = article.MetaDescription ?? article.Excerpt,
            Keywords = article.Keywords ?? article.Category,
            CanonicalPath = $"/blog/{article.Slug}",
            Type = "article"
        };
        ViewData["StructuredData"] = JsonSerializer.Serialize(new SeoArticleSchema
        {
            Headline = article.Title,
            Description = article.MetaDescription ?? article.Excerpt,
            Url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/blog/{article.Slug}",
            Image = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/Images/Logo/logo.png",
            DatePublished = article.PublishedAt,
            DateModified = article.UpdatedAt,
            AuthorName = article.AuthorName
        });

        return View(new BlogDetailsViewModel
        {
            Article = article,
            RelatedArticles = related,
            ContentBlocks = ParseContent(article.Content)
        });
    }

    private static IReadOnlyList<ArticleContentBlock> ParseContent(string content)
    {
        return content.Replace("\r\n", "\n")
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.StartsWith("## ", StringComparison.Ordinal)
                ? new ArticleContentBlock("heading", line[3..])
                : line.StartsWith("- ", StringComparison.Ordinal)
                    ? new ArticleContentBlock("list", line[2..])
                    : new ArticleContentBlock("paragraph", line))
            .ToList();
    }
}
