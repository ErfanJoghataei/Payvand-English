using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payvand.DAL.AppDbContext;
using Payvand.DAL.Entities;
using System.Security.Cryptography;

namespace Payvand.BusinessLogic
{
    public class CreateLink
    {
        private readonly PayvandDbContext context;
        private readonly ILogger<CreateLink> logger;

        public CreateLink(PayvandDbContext context, ILogger<CreateLink> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        private static string GenerateShortCode(int length = 6)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return RandomNumberGenerator.GetString(chars, length);
        }

        public async Task<ShortenedLink> Create(string originalurl, int? userid, int? guestid)
        {
            logger.LogInformation(
                "Creating shortened link. UserId: {UserId}, GuestId: {GuestId}, Url: {OriginalUrl}",
                userid,
                guestid,
                originalurl);

            string shortCode;
            do
            {
                shortCode = GenerateShortCode();
            }
            while (await context.ShortenedLinks.AnyAsync(c => c.ShorteCode == shortCode));

            var link = new ShortenedLink
            {
                OriginalUrl = originalurl,
                ShorteCode = shortCode,
                CreatedAt = DateTime.UtcNow,
                Domain = "Payvand.ir",
                ExpiresAt = DateTime.UtcNow.AddMonths(1),
                UserId = userid,
                GuestSessionId = guestid
            };

            await context.ShortenedLinks.AddAsync(link);

            await context.SaveChangesAsync();
            logger.LogInformation(
                "Shortened link created. LinkId: {LinkId}, ShortCode: {ShortCode}, UserId: {UserId}, GuestId: {GuestId}",
                link.Id,
                link.ShorteCode,
                userid,
                guestid);

            return link;
        }
    }
}
