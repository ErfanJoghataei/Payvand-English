using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payvand.DAL.AppDbContext;
using Payvand.DAL.Entities;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using DrawingColorTranslator = System.Drawing.ColorTranslator;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using ImageSharpPoint = SixLabors.ImageSharp.Point;

namespace Payvand.BusinessLogic.QrCode
{
    public class QrCodeServise : IQrCodeServise
    {
        private readonly string rootFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "qrcodes");
        private readonly PayvandDbContext context;
        private readonly ILogger<QrCodeServise> logger;

        public QrCodeServise(PayvandDbContext context, ILogger<QrCodeServise> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public Task<byte[]> CreateQrCodeWithLogoAsync(int? shortenedLinkId, string link, string foregroundColor, string backgroundColor, int size, byte[]? logoBytes)
        {
            logger.LogInformation(
                "Generating QR code. ShortenedLinkId: {ShortenedLinkId}, Size: {Size}, HasLogo: {HasLogo}",
                shortenedLinkId,
                size,
                logoBytes != null);

            using QRCodeGenerator generator = new QRCodeGenerator();
            string finalLink;

            if (!string.IsNullOrWhiteSpace(link))
            {
                finalLink = link.Trim();
            }
            else if (shortenedLinkId != null)
            {
                var shortenedLink = context.ShortenedLinks.FirstOrDefault(c => c.Id == shortenedLinkId);
                if (shortenedLink == null)
                {
                    logger.LogWarning("QR generation failed because shortened link was not found. ShortenedLinkId: {ShortenedLinkId}", shortenedLinkId);
                    throw new Exception("Shortened link not found");
                }

                if (string.IsNullOrWhiteSpace(shortenedLink.Domain))
                {
                    finalLink = shortenedLink.OriginalUrl;
                }
                else
                {
                    var domain = shortenedLink.Domain.TrimEnd('/');
                    if (!domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                        !domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        domain = $"https://{domain}";
                    }

                    finalLink = $"{domain}/{shortenedLink.ShorteCode}";
                }
            }
            else
            {
                logger.LogWarning("QR generation failed because link was empty. ShortenedLinkId: {ShortenedLinkId}", shortenedLinkId);
                throw new Exception("Link is empty");
            }

            using QRCodeData data = generator.CreateQrCode(finalLink, QRCodeGenerator.ECCLevel.H);
            PngByteQRCode qr = new PngByteQRCode(data);

            var dark = DrawingColorTranslator.FromHtml(foregroundColor);
            var light = DrawingColorTranslator.FromHtml(backgroundColor);

            // QRCoder expects pixels-per-module, not the final image width. Passing the
            // requested width directly can allocate multi-gigabyte images.
            var pixelsPerModule = Math.Clamp(size / 40, 4, 20);
            byte[] qrBytes = qr.GetGraphic(
                pixelsPerModule,
                dark,
                light,
                drawQuietZones: true);

            if (logoBytes == null)
            {
                logger.LogInformation("QR code generated without logo. ShortenedLinkId: {ShortenedLinkId}, Bytes: {Bytes}", shortenedLinkId, qrBytes.Length);
                return Task.FromResult(qrBytes);
            }

            var logoInfo = ImageSharpImage.Identify(logoBytes);
            if (logoInfo == null || logoInfo.Width > 1024 || logoInfo.Height > 1024)
            {
                throw new InvalidDataException("QR logo dimensions are invalid.");
            }

            using Image<Rgba32> qrImage = ImageSharpImage.Load<Rgba32>(qrBytes);
            using Image<Rgba32> logo = ImageSharpImage.Load<Rgba32>(logoBytes);

            int logoSize = qrImage.Width / 5;
            logo.Mutate(x => x.Resize(logoSize, logoSize));

            int xPos = (qrImage.Width - logoSize) / 2;
            int yPos = (qrImage.Height - logoSize) / 2;

            qrImage.Mutate(x => x.DrawImage(logo, new ImageSharpPoint(xPos, yPos), 1f));

            using MemoryStream ms = new MemoryStream();
            qrImage.SaveAsPng(ms);
            logger.LogInformation("QR code generated with logo. ShortenedLinkId: {ShortenedLinkId}, Bytes: {Bytes}", shortenedLinkId, ms.Length);

            return Task.FromResult(ms.ToArray());
        }

        public async Task<QRCodeEntity> SaveAsync(
            int? shortenedLinkId,
            string link,
            string foregroundColor,
            string backgroundColor,
            int size,
            byte[]? logoBytes,
            int userId)
        {
            logger.LogInformation(
                "Saving QR code. UserId: {UserId}, ShortenedLinkId: {ShortenedLinkId}, Size: {Size}, HasLogo: {HasLogo}",
                userId,
                shortenedLinkId,
                size,
                logoBytes != null);

            string userFolder = Path.Combine(rootFolder, userId.ToString());
            Directory.CreateDirectory(userFolder);

            string? logoPath = null;

            if (logoBytes != null)
            {
                var logoFileName = $"logo_{Guid.NewGuid():N}.png";
                logoPath = Path.Combine(userFolder, logoFileName);
                await File.WriteAllBytesAsync(logoPath, logoBytes);
                logoPath = $"/qrcodes/{userId}/{logoFileName}";
                logger.LogInformation("QR logo saved. UserId: {UserId}, LogoPath: {LogoPath}", userId, logoPath);
            }

            byte[] qrBytes = await CreateQrCodeWithLogoAsync(shortenedLinkId, link, foregroundColor, backgroundColor, size, logoBytes);

            var qrFileName = $"qrcode_{Guid.NewGuid():N}.png";
            string qrPath = Path.Combine(userFolder, qrFileName);
            await File.WriteAllBytesAsync(qrPath, qrBytes);
            var qrWebPath = $"/qrcodes/{userId}/{qrFileName}";

            var shlink = await context.ShortenedLinks.FirstOrDefaultAsync(c => c.Id == shortenedLinkId);

            QRCodeEntity qrEntity = new QRCodeEntity()
            {
                UserId = userId,
                ShortenedLinkId = shortenedLinkId,
                ShortenedLink = shlink,
                QrImagePath = qrWebPath,
                LogoPath = logoPath,
                ForeGroundColor = foregroundColor,
                BackGroundColor = backgroundColor,
                Size = size,
                CreatedAt = DateTime.UtcNow,
                Link = link
            };

            context.QrCodes.Add(qrEntity);
            await context.SaveChangesAsync();
            logger.LogInformation("QR code saved. QrCodeId: {QrCodeId}, UserId: {UserId}, Path: {QrImagePath}", qrEntity.Id, userId, qrEntity.QrImagePath);

            return qrEntity;
        }
    }
}
