using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payvand.DAL.AppDbContext;

namespace Payvand.UI.Services;

public sealed class UsageQuotaService
{
    private readonly PayvandDbContext db;
    private readonly UsageLimitsOptions limits;

    public UsageQuotaService(PayvandDbContext db, IOptions<UsageLimitsOptions> limits)
    {
        this.db = db;
        this.limits = limits.Value;
    }

    public async Task<bool> TryConsumeLinkAsync(int? userId, int? guestId, CancellationToken cancellationToken = default)
    {
        if (userId.HasValue)
        {
            var limit = Math.Max(1, limits.UserMonthlyLinks);
            var affected = await db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE [Users]
SET [LinkCount] = CASE
        WHEN [LastLinkQuotaResetAt] IS NULL OR DATEDIFF(month, [LastLinkQuotaResetAt], SYSUTCDATETIME()) <> 0 THEN 1
        ELSE [LinkCount] + 1 END,
    [LastLinkQuotaResetAt] = CASE
        WHEN [LastLinkQuotaResetAt] IS NULL OR DATEDIFF(month, [LastLinkQuotaResetAt], SYSUTCDATETIME()) <> 0 THEN SYSUTCDATETIME()
        ELSE [LastLinkQuotaResetAt] END
WHERE [UserId] = {userId.Value}
  AND ([LastLinkQuotaResetAt] IS NULL OR DATEDIFF(month, [LastLinkQuotaResetAt], SYSUTCDATETIME()) <> 0 OR [LinkCount] < {limit});", cancellationToken);
            return affected == 1;
        }

        if (!guestId.HasValue)
        {
            return false;
        }

        var guestLimit = Math.Max(1, limits.GuestMonthlyLinks);
        var guestAffected = await db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE [GuestSessions]
SET [LinkCount] = CASE
        WHEN [LastResetAt] IS NULL OR DATEDIFF(month, [LastResetAt], SYSUTCDATETIME()) <> 0 THEN 1
        ELSE [LinkCount] + 1 END,
    [LastResetAt] = CASE
        WHEN [LastResetAt] IS NULL OR DATEDIFF(month, [LastResetAt], SYSUTCDATETIME()) <> 0 THEN SYSUTCDATETIME()
        ELSE [LastResetAt] END
WHERE [Id] = {guestId.Value}
  AND ([LastResetAt] IS NULL OR DATEDIFF(month, [LastResetAt], SYSUTCDATETIME()) <> 0 OR [LinkCount] < {guestLimit});", cancellationToken);
        return guestAffected == 1;
    }

    public async Task<bool> TryConsumeQrCodeAsync(int? userId, int? guestId, CancellationToken cancellationToken = default)
    {
        if (userId.HasValue)
        {
            var limit = Math.Max(1, limits.UserMonthlyQrCodes);
            var affected = await db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE [Users]
SET [QrCodeCount] = CASE
        WHEN [LastQrQuotaResetAt] IS NULL OR DATEDIFF(month, [LastQrQuotaResetAt], SYSUTCDATETIME()) <> 0 THEN 1
        ELSE [QrCodeCount] + 1 END,
    [LastQrQuotaResetAt] = CASE
        WHEN [LastQrQuotaResetAt] IS NULL OR DATEDIFF(month, [LastQrQuotaResetAt], SYSUTCDATETIME()) <> 0 THEN SYSUTCDATETIME()
        ELSE [LastQrQuotaResetAt] END
WHERE [UserId] = {userId.Value}
  AND ([LastQrQuotaResetAt] IS NULL OR DATEDIFF(month, [LastQrQuotaResetAt], SYSUTCDATETIME()) <> 0 OR [QrCodeCount] < {limit});", cancellationToken);
            return affected == 1;
        }

        if (!guestId.HasValue)
        {
            return false;
        }

        var guestLimit = Math.Max(1, limits.GuestMonthlyQrCodes);
        var guestAffected = await db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE [GuestSessions]
SET [QrCodeCount] = CASE
        WHEN [LastQrQuotaResetAt] IS NULL OR DATEDIFF(month, [LastQrQuotaResetAt], SYSUTCDATETIME()) <> 0 THEN 1
        ELSE [QrCodeCount] + 1 END,
    [LastQrQuotaResetAt] = CASE
        WHEN [LastQrQuotaResetAt] IS NULL OR DATEDIFF(month, [LastQrQuotaResetAt], SYSUTCDATETIME()) <> 0 THEN SYSUTCDATETIME()
        ELSE [LastQrQuotaResetAt] END
WHERE [Id] = {guestId.Value}
  AND ([LastQrQuotaResetAt] IS NULL OR DATEDIFF(month, [LastQrQuotaResetAt], SYSUTCDATETIME()) <> 0 OR [QrCodeCount] < {guestLimit});", cancellationToken);
        return guestAffected == 1;
    }
}
