using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Payvand.BusinessLogic;
using Payvand.DAL.AppDbContext;
using Payvand.DAL.Entities;
using Payvand.UI.Security;
using System.Security.Claims;

namespace Payvand.UI.Controllers;

[EnableRateLimiting("authentication")]
public sealed class AccountController : Controller
{
    private readonly PayvandDbContext context;
    private readonly GenerateUserName generateUserName;
    private readonly IAuthenticationSchemeProvider schemeProvider;
    private readonly ILogger<AccountController> logger;

    public AccountController(
        PayvandDbContext context,
        GenerateUserName generateUserName,
        IAuthenticationSchemeProvider schemeProvider,
        ILogger<AccountController> logger)
    {
        this.context = context;
        this.generateUserName = generateUserName;
        this.schemeProvider = schemeProvider;
        this.logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null, string? error = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        ViewData["GoogleEnabled"] = await schemeProvider.GetSchemeAsync(AuthenticationSchemes.Google) != null;
        ViewData["Error"] = error switch
        {
            "google" => "Google sign in failed. Please try again.",
            "email" => "Google did not return a verified email for this account.",
            "account" => "This account cannot sign in.",
            _ => null
        };
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GoogleLogin(string? returnUrl = null)
    {
        if (await schemeProvider.GetSchemeAsync(AuthenticationSchemes.Google) == null)
        {
            return RedirectToAction(nameof(Login), new { error = "account", returnUrl });
        }

        await HttpContext.SignOutAsync(AuthenticationSchemes.External);
        var safeReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Dash", "Dashboard");
        var callbackUrl = Url.Action(nameof(GoogleCallback), "Account", new { returnUrl = safeReturnUrl });
        return Challenge(
            new AuthenticationProperties { RedirectUri = callbackUrl },
            AuthenticationSchemes.Google);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
    {
        var externalResult = await HttpContext.AuthenticateAsync(AuthenticationSchemes.External);
        if (!externalResult.Succeeded || externalResult.Principal == null)
        {
            return RedirectToAction(nameof(Login), new { error = "google", returnUrl });
        }

        var externalPrincipal = externalResult.Principal;
        var googleSubject = externalPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = externalPrincipal.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        var emailVerified = externalPrincipal.FindFirstValue("google:email_verified");

        if (string.IsNullOrWhiteSpace(googleSubject) ||
            string.IsNullOrWhiteSpace(email) ||
            !string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
        {
            await HttpContext.SignOutAsync(AuthenticationSchemes.External);
            return RedirectToAction(nameof(Login), new { error = "email", returnUrl });
        }

        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.GoogleSubject == googleSubject);

        if (user?.IsDeleted == true)
        {
            await HttpContext.SignOutAsync(AuthenticationSchemes.External);
            return RedirectToAction(nameof(Login), new { error = "account" });
        }

        if (user == null)
        {
            user = new User
            {
                GoogleSubject = googleSubject,
                Email = email,
                UserName = generateUserName.GenerateName(),
                DisplayName = externalPrincipal.FindFirstValue(ClaimTypes.Name),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            context.Users.Add(user);
        }
        else
        {
            user.Email = email;
            user.LastLoginAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        await ConvertGuestSessionAsync(user.UserId);
        await SignInUserAsync(user);
        await HttpContext.SignOutAsync(AuthenticationSchemes.External);

        logger.LogInformation("Google login completed. UserId: {UserId}", user.UserId);
        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthenticationSchemes.Application);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.DisplayName ?? user.UserName),
            new(ClaimTypes.Role, "User"),
            new("auth_provider", "google")
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationSchemes.Application));
        await HttpContext.SignInAsync(
            AuthenticationSchemes.Application,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                AllowRefresh = true
            });
    }

    private async Task ConvertGuestSessionAsync(int userId)
    {
        var tokenHash = GuestSessionCookie.GetHash(Request);
        if (tokenHash == null)
        {
            return;
        }

        var guest = await context.GuestSessions.SingleOrDefaultAsync(c => c.SessionTokenHash == tokenHash);
        if (guest == null)
        {
            return;
        }

        await context.ShortenedLinks
            .Where(c => c.GuestSessionId == guest.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.UserId, userId)
                .SetProperty(c => c.GuestSessionId, (int?)null));

        guest.UserId = userId;
        guest.ConvertedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        Response.Cookies.Delete(GuestSessionCookie.Name, GuestSessionCookie.CreateOptions(DateTimeOffset.UnixEpoch));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Dash", "Dashboard")!;
    }
}
