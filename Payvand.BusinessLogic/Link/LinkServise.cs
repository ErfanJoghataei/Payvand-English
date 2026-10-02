using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payvand.DAL.AppDbContext;
using Payvand.DAL.Entities;

namespace Payvand.BusinessLogic.Link
{
    public class LinkServise : ILinkServise
    {
        private readonly PayvandDbContext context;
        private readonly ILogger<LinkServise> logger;

        public LinkServise(PayvandDbContext context, ILogger<LinkServise> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<ICollection<ShortenedLink>> GetLinks(int userid)
        {
            logger.LogInformation("Loading links for user. UserId: {UserId}", userid);
            var user = await context.Users
                .Include(c => c.ShortenedLinks)
                .FirstOrDefaultAsync(c => c.UserId == userid);

            if (user == null)
            {
                logger.LogWarning("Links requested for missing user. UserId: {UserId}", userid);
                return Array.Empty<ShortenedLink>();
            }

            logger.LogInformation("Links loaded for user. UserId: {UserId}, Count: {Count}", userid, user.ShortenedLinks.Count);
            return user.ShortenedLinks;
        }

        public async Task RemoveLink(int linkid)
        {
            logger.LogInformation("Removing shortened link. LinkId: {LinkId}", linkid);
            var link = context.ShortenedLinks.FirstOrDefault(c => c.Id == linkid);
            if (link == null)
            {
                logger.LogWarning("Remove link skipped because link was not found. LinkId: {LinkId}", linkid);
                return;
            }

            try
            {
                context.ShortenedLinks.Remove(link);
                await context.SaveChangesAsync();
                logger.LogInformation("Shortened link removed. LinkId: {LinkId}", linkid);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Remove link failed. LinkId: {LinkId}", linkid);
                throw;
            }
        }
    }
}
