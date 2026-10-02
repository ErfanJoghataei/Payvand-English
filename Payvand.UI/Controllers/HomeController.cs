using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.VisualBasic;
using Payvand.BusinessLogic;
using Payvand.BusinessLogic.QrCode;
using Payvand.DAL.AppDbContext;
using Payvand.DAL.Entities;
using Payvand.UI.Models;
using Payvand.UI.Security;
using Payvand.UI.Services;
using System.Data;
using System.Formats.Asn1;
using System.Security.Claims;

namespace Payvand.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly CreateLink createLink;
        private readonly PayvandDbContext dbcontext;
        private readonly SafeBrowsingService safeBrowsing;
        private readonly IQrCodeServise qrCodeServise;
        private readonly ILogger<HomeController> logger;
        private readonly UsageQuotaService usageQuota;

        public HomeController(CreateLink createLink, PayvandDbContext dbcontext, SafeBrowsingService safeBrowsing, IQrCodeServise qrCodeServise, UsageQuotaService usageQuota, ILogger<HomeController> logger)
        {
            this.createLink = createLink;
            this.dbcontext = dbcontext;
            this.safeBrowsing = safeBrowsing;
            this.qrCodeServise = qrCodeServise;
            this.usageQuota = usageQuota;
            this.logger = logger;
        }



        [HttpGet]
        [HttpHead]
        public IActionResult Index(LinkViewModel model)
        {
            logger.LogInformation("Home page requested.");


            var userscount = dbcontext.Users.Count() + 56;
            var clickscount = dbcontext.ClickLogs.Count() + 256;
            var linksCount = dbcontext.ShortenedLinks.Count() + 124;

            var Vmodel = new LinkViewModel
            {
                UsersCount = userscount,
                TodayClicksCount = clickscount,
                LinksCount = linksCount

            };

            return View(Vmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("creation")]
        public async Task<IActionResult> LinkShortener(LinkViewModel model)
        {
            logger.LogInformation(
                "Home link shortener requested. UserId: {UserId}, UrlPresent: {UrlPresent}, UrlLength: {UrlLength}",
                GetCurrentUserId(),
                !string.IsNullOrWhiteSpace(model.OriginalUrl),
                model.OriginalUrl?.Length ?? 0);

            // Determine if this is an AJAX request for progressive enhancement fallback
            bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                                 Request.Headers["Accept"].ToString().Contains("application/json");

            // Helper: return JSON for AJAX, View for traditional form post
            IActionResult RespondJsonOrView(ShortenLinkResponse response, LinkViewModel? viewModel = null)
            {
                if (isAjaxRequest)
                {
                    return Json(response);
                }

                // Fallback: render the Index view with the model (traditional form post)
                var vm = viewModel ?? new LinkViewModel();
                if (!string.IsNullOrEmpty(response.Message))
                {
                    vm.ErrorMessage = response.Message;
                }
                vm.LoginRequired = response.LoginRequired;
                vm.ShortenedLink = response.ShortenedLink ?? string.Empty;
                if (response.ShortenerLinkId.HasValue)
                    vm.ShortenerLinkId = response.ShortenerLinkId.Value;
                vm.UsersCount = dbcontext.Users.Count();
                vm.TodayClicksCount = dbcontext.ClickLogs.Count();
                vm.LinksCount = dbcontext.ShortenedLinks.Count();
                return View(nameof(Index), vm);
            }

            if (string.IsNullOrWhiteSpace(model.OriginalUrl))
            {
                return RespondJsonOrView(new ShortenLinkResponse
                {
                    Success = false,
                    Message = "Enter a URL."
                }, model);
            }
            model.OriginalUrl = model.OriginalUrl.Trim();

            if (Uri.TryCreate(model.OriginalUrl, UriKind.Absolute, out var parsedUri))
            {
                if (!parsedUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                    !parsedUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    return RespondJsonOrView(new ShortenLinkResponse
                    {
                        Success = false,
                        Message = "Enter a valid URL."
                    }, model);
                }

                if (!PublicUrlValidator.IsAllowed(parsedUri, Request.Host.Host))
                {
                    return RespondJsonOrView(new ShortenLinkResponse
                    {
                        Success = false,
                        Message = "This address cannot be shortened."
                    }, model);
                }
                model.OriginalUrl = parsedUri.ToString();
            }
            else
            {
                if (Uri.TryCreate($"https://{model.OriginalUrl}", UriKind.Absolute, out var normalizedUri))
                {
                    if (!normalizedUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                        !normalizedUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                    {
                        return RespondJsonOrView(new ShortenLinkResponse
                        {
                            Success = false,
                            Message = "Enter a valid URL."
                        }, model);
                    }

                    if (!PublicUrlValidator.IsAllowed(normalizedUri, Request.Host.Host))
                    {
                        return RespondJsonOrView(new ShortenLinkResponse
                        {
                            Success = false,
                            Message = "This address cannot be shortened."
                        }, model);
                    }
                    model.OriginalUrl = normalizedUri.ToString();
                }
                else
                {
                    return RespondJsonOrView(new ShortenLinkResponse
                    {
                        Success = false,
                        Message = "Enter a valid URL."
                    }, model);
                }
            }

            bool issafe = await safeBrowsing.IsUrlSafe(model.OriginalUrl);

            if (!issafe)
            {
                return RespondJsonOrView(new ShortenLinkResponse
                {
                    Success = false,
                    Message = "This link is unsafe."
                }, model);
            }

            var userId = GetCurrentUserId();
            var guestId = userId.HasValue ? null : GetCurrentGuestId();
            if (!userId.HasValue && !guestId.HasValue)
            {
                return RespondJsonOrView(new ShortenLinkResponse
                {
                    Success = false,
                    Message = "Your session has expired. Reload the page."
                }, model);
            }

            await using var transaction = await dbcontext.Database.BeginTransactionAsync();
            if (!await usageQuota.TryConsumeLinkAsync(userId, guestId))
            {
                return RespondJsonOrView(new ShortenLinkResponse
                {
                    Success = false,
                    LoginRequired = !userId.HasValue,
                    Message = userId.HasValue
                        ? "You have reached your monthly link limit."
                        : "You have reached the guest limit for this month. Sign in for more links."
                }, model);
            }

            var newlink = await createLink.Create(model.OriginalUrl, userId, guestId);
            await transaction.CommitAsync();
            logger.LogInformation("Home shortened link created. LinkId: {LinkId}, ShortCode: {ShortCode}, UserId: {UserId}, GuestId: {GuestId}", newlink.Id, newlink.ShorteCode, userId, guestId);

            var shortenedLink = $"{Request.Scheme}://{Request.Host}/{newlink.ShorteCode}";

            model.ShortenedLink = shortenedLink;
            model.ShortenerLinkId = newlink.Id;
            model.UsersCount = dbcontext.Users.Count() + 56;
            model.TodayClicksCount = dbcontext.ClickLogs.Count() + 256;
            model.LinksCount = dbcontext.ShortenedLinks.Count() + 124;

            return RespondJsonOrView(new ShortenLinkResponse
            {
                Success = true,
                ShortenedLink = shortenedLink,
                ShortenerLinkId = newlink.Id
            }, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("creation")]
        public async Task<IActionResult> DownloadQrCode(int shortLinkId)
        {
            var userId = GetCurrentUserId();
            var guestId = userId.HasValue ? null : GetCurrentGuestId();
            var link = await dbcontext.ShortenedLinks.AsNoTracking().FirstOrDefaultAsync(c =>
                c.Id == shortLinkId &&
                (userId.HasValue ? c.UserId == userId : c.GuestSessionId == guestId));

            if (link == null)
            {
                return NotFound();
            }

            if (!await usageQuota.TryConsumeQrCodeAsync(userId, guestId))
            {
                TempData["QrError"] = userId.HasValue
                    ? "You have reached your monthly QR code limit."
                    : "Your guest QR code allowance is used up. Sign in for more.";
                return RedirectToAction(nameof(Index));
            }

            var target = $"{Request.Scheme}://{Request.Host}/{link.ShorteCode}";
            var bytes = await qrCodeServise.CreateQrCodeWithLogoAsync(
                link.Id, target, "#000000", "#FFFFFF", 300, null);
            return File(bytes, "image/png", $"payvand-{link.ShorteCode}.png");
        }

        [HttpGet]
        [EnableRateLimiting("creation")]
        public async Task<IActionResult> LinkAnalytics(int linkId)
        {
            var userId = GetCurrentUserId();
            var guestId = userId.HasValue ? null : GetCurrentGuestId();
            var link = await dbcontext.ShortenedLinks.AsNoTracking().FirstOrDefaultAsync(c =>
                c.Id == linkId &&
                (userId.HasValue ? c.UserId == userId : c.GuestSessionId == guestId));

            if (link == null)
            {
                return NotFound();
            }

            var recentClicks = await dbcontext.ClickLogs.AsNoTracking()
                .Where(c => c.LinkId == link.Id)
                .OrderByDescending(c => c.ClickedAt)
                .Take(20)
                .Select(c => new LinkClickSummary
                {
                    ClickedAt = c.ClickedAt,
                    Country = c.Country,
                    City = c.City,
                    Device = c.DeviceType.ToString(),
                    Browser = c.Browser
                })
                .ToListAsync();

            var model = new LinkAnalyticsViewModel
            {
                ShortUrl = $"{Request.Scheme}://{Request.Host}/{link.ShorteCode}",
                OriginalUrl = link.OriginalUrl,
                TotalClicks = await dbcontext.ClickLogs.CountAsync(c => c.LinkId == link.Id),
                RecentClicks = recentClicks
            };
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreateQrCode(int? shortLinkId)
        {
            var id = GetCurrentUserId();
            logger.LogInformation("Home create QR page requested. UserId: {UserId}, ShortLinkId: {ShortLinkId}", id, shortLinkId);
            if (id == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var links = await dbcontext.ShortenedLinks
                .Where(c => c.UserId == id.Value)
                .ToListAsync();

            ShortenedLink? shortenedlink = null;

            if (shortLinkId != null)
            {
                shortenedlink = await dbcontext.ShortenedLinks
                    .FirstOrDefaultAsync(x =>
                        x.Id == shortLinkId &&
                        x.UserId == id.Value);
            }

            var model = new QrCodeViewModel
            {
                UserId = id.Value,
                ShortenedLinks = links,
                SelectedShortenedLink = shortenedlink
            };

            return View(model);


        }

        public async Task<byte[]> ConvertToBytes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("creation")]
        [RequestSizeLimit(64_000)]
        public async Task<IActionResult> SendMessage(LinkViewModel model)
        {
            logger.LogInformation("Contact message submitted. UserId: {UserId}, Email: {Email}, Topic: {Topic}", GetCurrentUserId(), model.Messagemodel.Email, model.Messagemodel.Topic);
            if (!ModelState.IsValid)
            {
                TempData["ContactError"] = "Complete all message fields correctly.";
                return RedirectToAction(nameof(Index), "Home", null, "contact");
            }
            var message = new Message
            {
                FullName = model.Messagemodel.FullName,
                Email = model.Messagemodel.Email,
                Text = model.Messagemodel.Message,
                Topic = model.Messagemodel.Topic
            };
            message.UserId = GetCurrentUserId();
            await dbcontext.Messages.AddAsync(message);
            await dbcontext.SaveChangesAsync();
            logger.LogInformation("Contact message saved. MessageId: {MessageId}, UserId: {UserId}", message.Id, message.UserId);
            return View(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendViolationReport(LinkViewModel model)
        {
            ModelState.Clear();
            logger.LogInformation("Violation report submitted. UserId: {UserId}, GuestId: {GuestId}", GetCurrentUserId(), GetCurrentGuestId());
            if (model.ViolationReport == null || !TryValidateModel(model.ViolationReport, nameof(model.ViolationReport)))
            {
                TempData["ViolationReportError"] = "Complete the abuse report correctly.";
                return RedirectToAction(nameof(Index), "Home", null, "violation-report");
            }

            var normalizedUrl = model.ViolationReport.ReportedUrl.Trim();
            var shortenedLink = await ResolveShortenedLinkAsync(normalizedUrl);

            var report = new ViolationReport
            {
                UserId = GetCurrentUserId(),
                GuestSessionId = GetCurrentGuestId(),
                ShortenedLinkId = shortenedLink?.Id,
                ShortenedLink = shortenedLink,
                ReportedUrl = normalizedUrl,
                Reason = model.ViolationReport.Reason.Trim(),
                Description = model.ViolationReport.Description.Trim(),
                ReporterEmail = string.IsNullOrWhiteSpace(model.ViolationReport.ReporterEmail)
                    ? null
                    : model.ViolationReport.ReporterEmail.Trim(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await dbcontext.ViolationReports.AddAsync(report);
            await dbcontext.SaveChangesAsync();
            logger.LogInformation("Violation report saved. ReportId: {ReportId}, UserId: {UserId}, GuestId: {GuestId}, ShortenedLinkId: {ShortenedLinkId}, Reason: {Reason}", report.Id, report.UserId, report.GuestSessionId, report.ShortenedLinkId, report.Reason);

            TempData["ViolationReportSuccess"] = "Your report was submitted and will be reviewed.";
            return RedirectToAction(nameof(Index), "Home", null, "violation-report");
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdValue, out var userId) ? userId : null;
        }

        private int? GetCurrentGuestId()
        {
            return HttpContext.Items.TryGetValue("GuestSessionId", out var value) && value is int guestId
                ? guestId
                : null;
        }

        private async Task<ShortenedLink?> ResolveShortenedLinkAsync(string reportedUrl)
        {
            if (!Uri.TryCreate(reportedUrl, UriKind.Absolute, out var uri))
            {
                return null;
            }

            var code = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            return await dbcontext.ShortenedLinks.FirstOrDefaultAsync(c => c.ShorteCode == code);
        }

    }
}
