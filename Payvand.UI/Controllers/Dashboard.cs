using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Payvand.BusinessLogic;
using Payvand.BusinessLogic.Link;
using Payvand.BusinessLogic.QrCode;
using Payvand.DAL.AppDbContext;
using Payvand.DAL.Entities;
using Payvand.UI.Models;
using Payvand.UI.Security;
using Payvand.UI.Services;
using System.Security.Claims;

namespace Payvand.UI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly CreateLink createLink;
        private readonly SafeBrowsingService safeBrowsing;
        private readonly PayvandDbContext context;
        private readonly IQrCodeServise qrCodeServise;
        private readonly ILogger<DashboardController> logger;
        private readonly UsageQuotaService usageQuota;

        public DashboardController(CreateLink createLink, SafeBrowsingService safeBrowsing, PayvandDbContext context, ILinkServise linkServise, IQrCodeServise qrCodeServise, UsageQuotaService usageQuota, ILogger<DashboardController> logger)
        {
            this.createLink = createLink;
            this.safeBrowsing = safeBrowsing;
            this.context = context;
            this.qrCodeServise = qrCodeServise;
            this.usageQuota = usageQuota;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Dash(int? qrLinkId = null)
        {
            var userId = GetCurrentUserId();
            logger.LogInformation("Dashboard requested. UserId: {UserId}", userId);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await context.Users.FirstOrDefaultAsync(c => c.UserId == userId.Value);
            if (user == null)
            {
                logger.LogWarning("Dashboard requested but user was not found. UserId: {UserId}", userId);
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }

            var iranTimeZone = GetIranTimeZone();
            var todayIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, iranTimeZone).Date;
            var todayUtc = TimeZoneInfo.ConvertTimeToUtc(todayIran, iranTimeZone);
            var tomorrowUtc = TimeZoneInfo.ConvertTimeToUtc(todayIran.AddDays(1), iranTimeZone);

            var todayClicks = await context.ClickLogs
                .Where(c => c.UserId == user.UserId && c.ClickedAt >= todayUtc && c.ClickedAt < tomorrowUtc)
                .CountAsync();

            var allClicks = await context.ClickLogs
                .Where(c => c.UserId == user.UserId)
                .CountAsync();

            var linkCount = await context.ShortenedLinks
                .Where(c => c.UserId == user.UserId)
                .CountAsync();

            var lastlinks = await context.ShortenedLinks
                .Where(c => c.UserId == user.UserId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(4)
                .Include(c => c.ClickLogs)
                .ToListAsync();

            var shortenedlinks = await context.ShortenedLinks
                .Where(c => c.UserId == user.UserId)
                .OrderByDescending(c => c.CreatedAt)
                .Include(c => c.ClickLogs)
                .ToListAsync();

            var selectedQrLinkId = qrLinkId != null && shortenedlinks.Any(c => c.Id == qrLinkId.Value)
                ? qrLinkId
                : null;

            var qrCodes = await context.QrCodes
                .Where(c => c.UserId == user.UserId && !c.ISDeleted)
                .Include(c => c.ShortenedLink)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                LinkCount = linkCount,
                TodayClicks = todayClicks,
                AllClicks = allClicks,
                LastLinks = lastlinks,
                ShortenedLinks = shortenedlinks,
                DisplayName = user.DisplayName,
                LastLogin = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                ShortenedLink = TempData["ShortenedLink"] as string ?? string.Empty,
                Qrcdoe = new QrCodeViewModel
                {
                    UserId = user.UserId,
                    ShortenedLinks = shortenedlinks,
                    ShortenedLinkId = selectedQrLinkId,
                    SelectedShortenedLink = selectedQrLinkId == null
                        ? null
                        : shortenedlinks.FirstOrDefault(c => c.Id == selectedQrLinkId.Value),
                    QrCodes = qrCodes,
                    Count = qrCodes.Count
                }
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LinkAnalytics(int linkId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ownsLink = await context.ShortenedLinks
                .AnyAsync(c => c.Id == linkId && c.UserId == userId.Value && !c.IsDeleted);

            if (!ownsLink)
            {
                return RedirectToAction(nameof(Dash), "Dashboard", null, "analytics");
            }

            return RedirectToAction(nameof(Dash), "Dashboard", null, $"analytics-{linkId}");
        }

        [HttpGet]
        public async Task<IActionResult> CreateQrForLink(int shortLinkId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ownsLink = await context.ShortenedLinks
                .AnyAsync(c => c.Id == shortLinkId && c.UserId == userId.Value && !c.IsDeleted);

            if (!ownsLink)
            {
                return RedirectToAction(nameof(Dash), "Dashboard", null, "create");
            }

            return RedirectToAction(nameof(Dash), "Dashboard", new { qrLinkId = shortLinkId }, "create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("creation")]
        public async Task<IActionResult> CreateLinkDash(DashboardViewModel model)
        {
            var userId = GetCurrentUserId();

            logger.LogInformation(
                "Dashboard create link requested. UserId: {UserId}, Url: {Url}",
                userId,
                model.OriginalUrl);

            if (userId == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Sign in first."
                });
            }

            model.OriginalUrl = model.OriginalUrl.Trim();

            if (!model.OriginalUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !model.OriginalUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                model.OriginalUrl = "https://" + model.OriginalUrl;
            }

            if (!Uri.TryCreate(model.OriginalUrl, UriKind.Absolute, out var uri) ||
                !PublicUrlValidator.IsAllowed(uri, Request.Host.Host))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Enter a valid URL."
                });
            }

            var isSafe = await safeBrowsing.IsUrlSafe(model.OriginalUrl);

            if (!isSafe)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "This link is unsafe."
                });
            }

            await using var transaction = await context.Database.BeginTransactionAsync();
            if (!await usageQuota.TryConsumeLinkAsync(userId.Value, null))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    success = false,
                    message = "You have reached your monthly link limit."
                });
            }

            var link = await createLink.Create(model.OriginalUrl, userId.Value, null);
            await transaction.CommitAsync();

            logger.LogInformation(
                "Dashboard link created. UserId: {UserId}, LinkId: {LinkId}, ShortCode: {ShortCode}",
                userId,
                link.Id,
                link.ShorteCode);

            return Ok(new
            {
                success = true,
                shortenedLink = $"{Request.Host}/{link.ShorteCode}",
                shortCode = link.ShorteCode
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLink(DashboardViewModel model)
        {
            var userId = GetCurrentUserId();

            logger.LogInformation(
                "Dashboard remove link requested. UserId: {UserId}, LinkId: {LinkId}",
                userId,
                model.LinkId);

            if (userId == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Sign in first."
                });
            }

            var link = await context.ShortenedLinks
                .FirstOrDefaultAsync(x => x.Id == model.LinkId &&
                                          x.UserId == userId.Value);

            if (link == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Link not found."
                });
            }

            context.ShortenedLinks.Remove(link);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Dashboard link removed. UserId: {UserId}, LinkId: {LinkId}",
                userId,
                model.LinkId);

            return Ok(new
            {
                success = true,
                message = "Link deleted successfully."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserName(DashboardViewModel model)
        {
            var user = await GetCurrentUserAsync();
            logger.LogInformation("Display name change requested. UserId: {UserId}, DisplayName: {DisplayName}", user?.UserId, model.DisplayName);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var displayName = model.DisplayName?.Trim();
            if (string.IsNullOrWhiteSpace(displayName) || displayName.Length < 3 || displayName.Length > 30)
            {
                TempData["ChangeUserNameError"] = "Username must be 3 to 30 characters.";
                return RedirectToAction(nameof(Dash), "Dashboard", null, "profile");
            }

            var isTaken = await context.Users.AnyAsync(c =>
                c.UserId != user.UserId &&
                (c.DisplayName == displayName || c.UserName == displayName));

            if (isTaken)
            {
                TempData["ChangeUserNameError"] = "This username is already taken.";
                return RedirectToAction(nameof(Dash), "Dashboard", null, "profile");
            }

            user.DisplayName = displayName;
            await context.SaveChangesAsync();
            logger.LogInformation("Display name changed. UserId: {UserId}, DisplayName: {DisplayName}", user.UserId, displayName);
            return RedirectToAction(nameof(Dash), "Dashboard", null, "profile");
        }

        [HttpGet]
        public async Task<IActionResult> CheckDisplayName(string? displayName)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            displayName = displayName?.Trim();
            if (string.IsNullOrWhiteSpace(displayName) || displayName.Length < 3 || displayName.Length > 30)
            {
                return Json(new { available = false, message = "Username must be 3 to 30 characters." });
            }

            var isTaken = await context.Users.AnyAsync(c =>
                c.UserId != userId.Value &&
                (c.DisplayName == displayName || c.UserName == displayName));

            return Json(new
            {
                available = !isTaken,
                message = isTaken ? "This username is already taken." : "This username is available."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(3_000_000)]
        [EnableRateLimiting("creation")]
        public async Task<IActionResult> CreateQrCode([Bind(Prefix = "Qrcdoe")] QrCodeViewModel model)
        {
            var userId = GetCurrentUserId();

            logger.LogInformation(
                "QR create/edit requested. UserId: {UserId}, QrCodeId: {QrCodeId}, ShortenedLinkId: {ShortenedLinkId}, HasDirectLink: {HasDirectLink}",
                userId,
                model.Id,
                model.ShortenedLinkId,
                !string.IsNullOrWhiteSpace(model.Link));

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (model.ShortenedLinkId == null && string.IsNullOrWhiteSpace(model.Link))
            {
                TempData["QrError"] = "Choose a short link or enter a new URL to create a QR code.";
                return RedirectToAction(nameof(Dash), "Dashboard", null, "create");
            }

            var selectedShortLink = model.ShortenedLinkId == null
                ? null
                : await context.ShortenedLinks
                    .FirstOrDefaultAsync(c =>
                        c.Id == model.ShortenedLinkId.Value &&
                        c.UserId == userId.Value &&
                        !c.IsDeleted);

            if (model.ShortenedLinkId != null && selectedShortLink == null)
            {
                TempData["QrError"] = "The selected link was not found.";
                return RedirectToAction(nameof(Dash), "Dashboard", null, "create");
            }

            string targetLink;

            if (selectedShortLink != null)
            {
                targetLink = $"{Request.Scheme}://{Request.Host}/{selectedShortLink.ShorteCode}";
            }
            else
            {
                targetLink = (model.Link ?? string.Empty).Trim();

                if (!targetLink.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !targetLink.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    targetLink = "https://" + targetLink;
                }

                if (!Uri.TryCreate(targetLink, UriKind.Absolute, out var uri) ||
                    string.IsNullOrWhiteSpace(uri.Host) ||
                    !uri.Host.Contains('.'))
                {
                    TempData["QrError"] = "Enter a valid URL for the QR code.";
                    return RedirectToAction(nameof(Dash), "Dashboard", null, "create");
                }
            }

            byte[]? logoBytes = null;

            if (model.Logo != null && model.Logo.Length > 0)
            {
                if (model.Logo.Length > 2_000_000 ||
                    !new[] { "image/png", "image/jpeg", "image/webp" }.Contains(model.Logo.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    TempData["QrError"] = "Logo must be PNG, JPEG, or WebP and no larger than 2 MB.";
                    return RedirectToAction(nameof(Dash), "Dashboard", null, "create");
                }

                using var ms = new MemoryStream();
                await model.Logo.CopyToAsync(ms);
                logoBytes = ms.ToArray();
            }

            model.Size = Math.Clamp(model.Size, 180, 900);
            model.ForeGroundColor = string.IsNullOrWhiteSpace(model.ForeGroundColor)
                ? "#000000"
                : model.ForeGroundColor;

            model.BackGroundColor = string.IsNullOrWhiteSpace(model.BackGroundColor)
                ? "#FFFFFF"
                : model.BackGroundColor;

            if (!System.Text.RegularExpressions.Regex.IsMatch(model.ForeGroundColor, "^#[0-9a-fA-F]{6}$") ||
                !System.Text.RegularExpressions.Regex.IsMatch(model.BackGroundColor, "^#[0-9a-fA-F]{6}$"))
            {
                TempData["QrError"] = "Choose a valid QR code color.";
                return RedirectToAction(nameof(Dash), "Dashboard", null, "create");
            }

            if (model.Id > 0)
            {
                var existingQr = await context.QrCodes
                    .FirstOrDefaultAsync(c =>
                        c.Id == model.Id &&
                        c.UserId == userId.Value &&
                        !c.ISDeleted);

                if (existingQr == null)
                {
                    TempData["QrError"] = "The QR code to edit was not found.";
                    return RedirectToAction(nameof(Dash), "Dashboard", null, "create");
                }

                var qrBytes = await qrCodeServise.CreateQrCodeWithLogoAsync(
                    model.ShortenedLinkId,
                    targetLink,
                    model.ForeGroundColor,
                    model.BackGroundColor,
                    model.Size,
                    logoBytes);

                var userFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "qrcodes",
                    userId.Value.ToString());

                Directory.CreateDirectory(userFolder);

                var fileName = $"qrcode_{Guid.NewGuid():N}.png";

                await System.IO.File.WriteAllBytesAsync(
                    Path.Combine(userFolder, fileName),
                    qrBytes);

                existingQr.ShortenedLinkId = model.ShortenedLinkId;
                existingQr.ShortenedLink = selectedShortLink;
                existingQr.Link = targetLink;
                existingQr.ForeGroundColor = model.ForeGroundColor;
                existingQr.BackGroundColor = model.BackGroundColor;
                existingQr.Size = model.Size;
                existingQr.QrImagePath = $"/qrcodes/{userId.Value}/{fileName}";

                await context.SaveChangesAsync();

                TempData["QrSuccess"] = "QR code updated successfully.";

                logger.LogInformation(
                    "QR code edited. UserId: {UserId}, QrCodeId: {QrCodeId}",
                    userId,
                    existingQr.Id);
            }
            else
            {
                if (!await usageQuota.TryConsumeQrCodeAsync(userId.Value, null))
                {
                    TempData["QrError"] = "You have reached your monthly QR code limit.";
                    return RedirectToAction(nameof(Dash), "Dashboard", null, "create");
                }

                var qr = await qrCodeServise.SaveAsync(
                    model.ShortenedLinkId,
                    targetLink,
                    model.ForeGroundColor,
                    model.BackGroundColor,
                    model.Size,
                    logoBytes,
                    userId.Value);

                TempData["QrSuccess"] = "QR code created successfully.";

                logger.LogInformation(
                    "QR code created. UserId: {UserId}, QrCodeId: {QrCodeId}",
                    userId,
                    qr.Id);
            }

            return RedirectToAction(nameof(Dash), "Dashboard", null, "create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(3_000_000)]
        [EnableRateLimiting("creation")]
        public async Task<IActionResult> QrCodePreview([Bind(Prefix = "Qrcdoe")] QrCodeViewModel model)
        {
            var userId = GetCurrentUserId();
            logger.LogDebug("QR preview requested. UserId: {UserId}, ShortenedLinkId: {ShortenedLinkId}, HasDirectLink: {HasDirectLink}", userId, model.ShortenedLinkId, !string.IsNullOrWhiteSpace(model.Link));
            if (userId == null)
            {
                return Unauthorized();
            }

            if (model.ShortenedLinkId == null && string.IsNullOrWhiteSpace(model.Link))
            {
                return BadRequest();
            }

            var selectedShortLink = model.ShortenedLinkId == null
                ? null
                : await context.ShortenedLinks
                    .FirstOrDefaultAsync(c => c.Id == model.ShortenedLinkId.Value && c.UserId == userId.Value && !c.IsDeleted);

            if (model.ShortenedLinkId != null && selectedShortLink == null)
            {
                return BadRequest();
            }

            string targetLink;

            if (selectedShortLink != null)
            {
                targetLink = $"{Request.Scheme}://{Request.Host}/{selectedShortLink.ShorteCode}";
            }
            else
            {
                targetLink = (model.Link ?? string.Empty).Trim();

                if (!targetLink.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !targetLink.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    targetLink = "https://" + targetLink;
                }
            }

            if (!Uri.TryCreate(targetLink, UriKind.Absolute, out _))
            {
                return BadRequest();
            }

            byte[]? logoBytes = null;
            if (model.Logo != null && model.Logo.Length > 0)
            {
                if (model.Logo.Length > 2_000_000 ||
                    !new[] { "image/png", "image/jpeg", "image/webp" }.Contains(model.Logo.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest();
                }

                using var ms = new MemoryStream();
                await model.Logo.CopyToAsync(ms);
                logoBytes = ms.ToArray();
            }

            var size = Math.Clamp(model.Size, 180, 900);
            var foregroundColor = string.IsNullOrWhiteSpace(model.ForeGroundColor) ? "#000000" : model.ForeGroundColor;
            var backgroundColor = string.IsNullOrWhiteSpace(model.BackGroundColor) ? "#FFFFFF" : model.BackGroundColor;
            if (!System.Text.RegularExpressions.Regex.IsMatch(foregroundColor, "^#[0-9a-fA-F]{6}$") ||
                !System.Text.RegularExpressions.Regex.IsMatch(backgroundColor, "^#[0-9a-fA-F]{6}$"))
            {
                return BadRequest();
            }

            var qrBytes = await qrCodeServise.CreateQrCodeWithLogoAsync(model.ShortenedLinkId, targetLink, foregroundColor, backgroundColor, size, logoBytes);
            return File(qrBytes, "image/png");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQrCode(int id)
        {
            var userId = GetCurrentUserId();
            logger.LogInformation("QR delete requested. UserId: {UserId}, QrCodeId: {QrCodeId}", userId, id);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var qrCode = await context.QrCodes.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId.Value && !c.ISDeleted);
            if (qrCode != null)
            {
                qrCode.ISDeleted = true;
                await context.SaveChangesAsync();
                TempData["QrSuccess"] = "QR code deleted.";
                logger.LogInformation("QR code soft deleted. UserId: {UserId}, QrCodeId: {QrCodeId}", userId, id);
            }
            else
            {
                logger.LogWarning("QR delete skipped because QR code was not found or not owned. UserId: {UserId}, QrCodeId: {QrCodeId}", userId, id);
            }

            return RedirectToAction(nameof(Dash), "Dashboard", null, "create");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadQrCode(int id)
        {
            var userId = GetCurrentUserId();
            logger.LogInformation("QR download requested. UserId: {UserId}, QrCodeId: {QrCodeId}", userId, id);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var qrCode = await context.QrCodes.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId.Value && !c.ISDeleted);
            if (qrCode == null)
            {
                logger.LogWarning("QR download failed because QR code was not found. UserId: {UserId}, QrCodeId: {QrCodeId}", userId, id);
                return NotFound();
            }

            var relativePath = qrCode.QrImagePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
            if (!System.IO.File.Exists(fullPath))
            {
                logger.LogError("QR download failed because file does not exist. UserId: {UserId}, QrCodeId: {QrCodeId}, Path: {Path}", userId, id, fullPath);
                return NotFound();
            }

            var fileName = $"payvand-qrcode-{qrCode.Id}.png";
            return PhysicalFile(fullPath, "image/png", fileName);
        }


        private async Task<Payvand.DAL.Entities.User?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return null;
            }

            return await context.Users.FindAsync(userId.Value);
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdValue, out var userId) ? userId : null;
        }

        private static TimeZoneInfo GetIranTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
            }
        }
    }
}
