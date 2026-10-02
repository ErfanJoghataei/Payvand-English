using Microsoft.EntityFrameworkCore;
using Payvand.DAL.AppDbContext;
using Payvand.DAL.Entities;
using Payvand.UI.Security;
using System.Security.Claims;

namespace Payvand.UI.Middleware;

public sealed class GuestMiddleware
{
    private readonly RequestDelegate next;

    public GuestMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, PayvandDbContext db, ILogger<GuestMiddleware> logger)
    {
        if (ShouldSkipGuestTracking(context.Request))
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            await next(context);
            return;
        }

        var tokenHash = GuestSessionCookie.GetHash(context.Request);
        var guest = tokenHash == null
            ? null
            : await db.GuestSessions.SingleOrDefaultAsync(c => c.SessionTokenHash == tokenHash);

        if (guest == null)
        {
            var token = GuestSessionCookie.CreateToken();
            guest = new GuestSession
            {
                SessionTokenHash = GuestSessionCookie.Hash(token),
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                Referrer = context.Request.Headers.Referer.ToString(),
                FirstSeenAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            };

            db.GuestSessions.Add(guest);
            await db.SaveChangesAsync();
            context.Response.Cookies.Append(GuestSessionCookie.Name, token, GuestSessionCookie.CreateOptions());
            logger.LogDebug("Guest session created. GuestId: {GuestId}", guest.Id);
        }
        else if (guest.LastSeenAt < DateTime.UtcNow.AddMinutes(-15))
        {
            guest.LastSeenAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        context.Items["GuestSessionId"] = guest.Id;

        // While sign-in is disabled, create an isolated persistent identity for
        // this browser. Existing authorization and ownership checks remain active.
        var authenticationEnabled = context.RequestServices
            .GetRequiredService<IConfiguration>()
            .GetValue("Authentication:Enabled", true);

        if (!authenticationEnabled)
        {
            var user = guest.UserId.HasValue
                ? await db.Users.FirstOrDefaultAsync(c => c.UserId == guest.UserId.Value)
                : null;

            // A guest cookie previously converted to a real account must never
            // become a passwordless route back into that account.
            if (user != null && user.UserName != $"guest_session_{guest.Id}")
            {
                var replacementToken = GuestSessionCookie.CreateToken();
                guest = new GuestSession
                {
                    SessionTokenHash = GuestSessionCookie.Hash(replacementToken),
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    Referrer = context.Request.Headers.Referer.ToString(),
                    FirstSeenAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                };
                db.GuestSessions.Add(guest);
                await db.SaveChangesAsync();
                context.Response.Cookies.Append(GuestSessionCookie.Name, replacementToken, GuestSessionCookie.CreateOptions());
                context.Items["GuestSessionId"] = guest.Id;
                user = null;
            }

            if (user == null)
            {
                user = new User
                {
                    UserName = $"guest_session_{guest.Id}",
                    DisplayName = "کاربر مهمان",
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                guest.UserId = user.UserId;
                await db.SaveChangesAsync();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.DisplayName ?? user.UserName),
                new(ClaimTypes.Role, "User"),
                new("auth_provider", "temporary_guest")
            };
            context.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, AuthenticationSchemes.Application));
        }

        await next(context);
    }

    private static bool ShouldSkipGuestTracking(HttpRequest request)
    {
        var path = request.Path.Value ?? string.Empty;
        if (HttpMethods.IsHead(request.Method) ||
            IsShortCodePath(path))
        {
            return true;
        }

        if (path.StartsWith("/signin-google", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/robots.txt", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/sitemap.xml", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return HttpMethods.IsGet(request.Method) && Path.HasExtension(path);
    }

    private static bool IsAsciiLetterOrDigit(char value) =>
        value is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9';

    private static bool IsShortCodePath(string path)
    {
        if (path.Length != 7 || path[0] != '/')
        {
            return false;
        }

        for (var index = 1; index < path.Length; index++)
        {
            if (!IsAsciiLetterOrDigit(path[index]))
            {
                return false;
            }
        }

        return true;
    }
}
