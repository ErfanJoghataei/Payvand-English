using System.Text;
using System.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payvand.DAL.AppDbContext;

namespace Payvand.UI.Controllers;

public sealed class SeoController : Controller
{
    private readonly PayvandDbContext db;

    public SeoController(PayvandDbContext db)
    {
        this.db = db;
    }

    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public ContentResult Robots()
    {
        var baseUrl = BaseUrl();
        var robots = $"""
        User-agent: *
        Allow: /
        Disallow: /Dashboard
        Disallow: /Account

        Sitemap: {baseUrl}/sitemap.xml
        """;

        return Content(robots, "text/plain", Encoding.UTF8);
    }

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<ContentResult> Sitemap()
    {
        var baseUrl = BaseUrl();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var urls = new[]
        {
            new SitemapUrl("/", "daily", "1.0"),
            new SitemapUrl("/blog", "daily", "0.9"),
            new SitemapUrl("/Terms_Privacy/Terms", "monthly", "0.6"),
            new SitemapUrl("/Terms_Privacy/Privacy", "monthly", "0.6")
        };

        var xml = new StringBuilder();
        xml.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        xml.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");

        foreach (var url in urls)
        {
            xml.AppendLine("  <url>");
            xml.AppendLine($"    <loc>{baseUrl}{url.Path}</loc>");
            xml.AppendLine($"    <lastmod>{today}</lastmod>");
            xml.AppendLine($"    <changefreq>{url.ChangeFrequency}</changefreq>");
            xml.AppendLine($"    <priority>{url.Priority}</priority>");
            xml.AppendLine("  </url>");
        }

        var articles = await db.Articles.AsNoTracking()
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new { c.Slug, c.UpdatedAt })
            .ToListAsync();

        foreach (var article in articles)
        {
            xml.AppendLine("  <url>");
            xml.AppendLine($"    <loc>{SecurityElement.Escape($"{baseUrl}/blog/{article.Slug}")}</loc>");
            xml.AppendLine($"    <lastmod>{article.UpdatedAt:yyyy-MM-dd}</lastmod>");
            xml.AppendLine("    <changefreq>monthly</changefreq>");
            xml.AppendLine("    <priority>0.8</priority>");
            xml.AppendLine("  </url>");
        }

        xml.AppendLine("</urlset>");
        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }

    private string BaseUrl()
    {
        return $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');
    }

    private sealed record SitemapUrl(string Path, string ChangeFrequency, string Priority);
}
