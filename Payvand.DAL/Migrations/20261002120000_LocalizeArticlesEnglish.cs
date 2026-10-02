using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Payvand.DAL.AppDbContext;

namespace Payvand.DAL.Migrations;

[DbContext(typeof(PayvandDbContext))]
[Migration("20261002120000_LocalizeArticlesEnglish")]
public sealed class LocalizeArticlesEnglish : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
UPDATE [Articles] SET
[Title] = N'What Is a Short Link and Why Does It Matter?',
[Excerpt] = N'Learn how short URLs work and how to use them for sharing and measurement.',
[Content] = N'A short link is a compact version of a long URL. When someone opens it, the service redirects them to the original page. Short links make messages easier to read and can show how a campaign performs.
## How a short link works
The service assigns a unique code to your original URL. The short URL points to that code, and the service sends visitors to the saved destination.
## Why teams use them
- Cleaner links in messages and social posts
- Easier campaign tracking
- A single place to manage destinations and QR codes
## Good practice
Tell people where the link leads. Check the destination on mobile before publishing, and use a trusted domain. A short link helps distribution and measurement, but the destination page still needs to be useful and secure.',
[Category] = N'Short Links', [AuthorName] = N'Payvand Team',
[MetaTitle] = N'What Is a Short Link? | Payvand',
[MetaDescription] = N'How short links work and why they help with sharing and analytics.',
[Keywords] = N'short link, URL shortener, link analytics'
WHERE [Slug] = N'what-is-short-link';

UPDATE [Articles] SET
[Title] = N'How to Make a QR Code People Can Scan',
[Excerpt] = N'A practical guide to QR code size, contrast, destinations, and testing.',
[Content] = N'QR codes connect printed materials to digital pages. A useful code begins with a clear destination and a reason for someone to scan it.
## Start with the destination
Make sure the page loads quickly on a phone, uses HTTPS, and matches the promise next to the code.
## Design for readability
Use a dark foreground on a light background. Keep quiet space around the code, and avoid a logo that covers too much of the pattern.
## Test before publishing
- Scan with more than one phone
- Test at the final print size
- Check the code in different lighting
## Measure the result
Use a distinct short link for each placement to see which poster, package, or event drives visits.',
[Category] = N'QR Codes', [AuthorName] = N'Payvand Team',
[MetaTitle] = N'QR Code Design Guide | Payvand',
[MetaDescription] = N'Create readable QR codes with the right size, contrast, and destination.',
[Keywords] = N'QR code, QR design, scannable code'
WHERE [Slug] = N'qr-code-complete-guide';

UPDATE [Articles] SET
[Title] = N'How to Read Your Link Analytics',
[Excerpt] = N'Turn click counts, timing, devices, and sources into better decisions.',
[Content] = N'A click count is a starting point. To understand performance, look at when people visit, which devices they use, and where traffic comes from.
## Begin with a question
Decide whether you want to compare channels, publishing times, or two versions of a message.
## Useful signals
- Total clicks and the trend over time
- Mobile and desktop share
- Visitor locations when available
- Referring pages and campaign sources
## Compare fairly
Only compare links shared over similar periods with similar budgets and audiences. A useful report should lead to an action, such as improving the destination page or changing the publishing time.',
[Category] = N'Analytics', [AuthorName] = N'Payvand Team',
[MetaTitle] = N'Link Analytics Guide | Payvand',
[MetaDescription] = N'Use click analytics to improve campaigns and sharing.',
[Keywords] = N'link analytics, click tracking, campaign measurement'
WHERE [Slug] = N'analyze-link-clicks';

UPDATE [Articles] SET
[Title] = N'Seven Safety Checks Before Opening a Short Link',
[Excerpt] = N'Quick habits that help you spot suspicious short URLs and phishing pages.',
[Content] = N'Short links are convenient, but they hide their destination until you open them. A few checks can reduce the risk.
## Before you click
- Consider whether you expected the message
- Check the sender through a separate channel
- Look for unusual urgency or promises
- Prefer familiar, trusted shortening domains
## On the destination page
Check the domain in the address bar. Do not enter credentials or payment details on a page you do not trust. Keep your browser and device updated.
## When in doubt
Stop and navigate to the organization directly instead of following the link. Report suspicious short links to the service.',
[Category] = N'Security', [AuthorName] = N'Payvand Team',
[MetaTitle] = N'Short Link Safety Tips | Payvand',
[MetaDescription] = N'Seven ways to check short links and avoid phishing pages.',
[Keywords] = N'short link safety, phishing, suspicious URL'
WHERE [Slug] = N'short-link-security-tips';

UPDATE [Articles] SET
[Title] = N'Five Practical QR Code Marketing Ideas',
[Excerpt] = N'Use QR codes on packaging, in stores, at events, and in campaigns.',
[Content] = N'A QR code works best when it makes one useful action easier. Put a clear instruction beside it so people know what they will get after scanning.
## Product packaging
Link to setup instructions, product details, or a feedback form.
## Stores and events
- Open a digital menu or catalogue
- Register visitors for an event
- Deliver a relevant offer
- Collect feedback after a visit
## Measure each placement
Create a separate short link for each poster, location, or package. Compare scans to learn which placements create engagement.
## Keep it useful
Make the destination mobile friendly and keep the code large enough to scan at the expected distance.',
[Category] = N'Marketing', [AuthorName] = N'Payvand Team',
[MetaTitle] = N'QR Code Marketing Ideas | Payvand',
[MetaDescription] = N'Practical QR code uses for stores, packaging, events, and campaigns.',
[Keywords] = N'QR code marketing, retail QR, campaign ideas'
WHERE [Slug] = N'qr-code-marketing-ideas';
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The previous migration contains the original language content.
    }
}
