using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Payvand.BusinessLogic;
using Payvand.BusinessLogic.Link;
using Payvand.BusinessLogic.QrCode;
using Payvand.DAL.AppDbContext;
using Payvand.UI.Middleware;
using Payvand.UI.Security;
using Payvand.UI.Services;
using Serilog;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();

var authentication = builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = AuthenticationSchemes.Application;
        options.DefaultSignInScheme = AuthenticationSchemes.Application;
        options.DefaultChallengeScheme = AuthenticationSchemes.Application;
    })
    .AddCookie(AuthenticationSchemes.Application, options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.Name = "__Host-Payvand.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Path = "/";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events.OnValidatePrincipal = async validationContext =>
        {
            var userIdValue = validationContext.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var db = validationContext.HttpContext.RequestServices.GetRequiredService<PayvandDbContext>();
            if (!int.TryParse(userIdValue, out var userId) ||
                !await db.Users.AsNoTracking().AnyAsync(c => c.UserId == userId))
            {
                validationContext.RejectPrincipal();
                await validationContext.HttpContext.SignOutAsync(AuthenticationSchemes.Application);
            }
        };
    })
    .AddCookie(AuthenticationSchemes.External, options =>
    {
        options.Cookie.Name = "__Host-Payvand.External";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Path = "/";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    });

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
var authenticationEnabled = builder.Configuration.GetValue("Authentication:Enabled", true);
if (authenticationEnabled &&
    !string.IsNullOrWhiteSpace(googleClientId) &&
    !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authentication.AddGoogle(AuthenticationSchemes.Google, options =>
    {
        options.SignInScheme = AuthenticationSchemes.External;
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.SaveTokens = false;
        options.ClaimActions.MapJsonKey("google:email_verified", "verified_email");
        options.CorrelationCookie.HttpOnly = true;
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events.OnRemoteFailure = remoteContext =>
        {
            remoteContext.HandleResponse();
            remoteContext.Response.Redirect("/Account/Login?error=google");
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("authentication", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddPolicy("creation", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
var cnnstring = builder.Configuration.GetConnectionString("cnnstring");

builder.Services.AddDbContext<PayvandDbContext>(option => option.UseSqlServer(cnnstring));
builder.Services.AddScoped<GenerateUserName>();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<SafeBrowsingService>();
builder.Services.AddScoped<CreateLink>();
builder.Services.AddScoped<ILinkServise, LinkServise>();
builder.Services.AddScoped<IQrCodeServise, QrCodeServise>();
builder.Services.Configure<UsageLimitsOptions>(builder.Configuration.GetSection("UsageLimits"));
builder.Services.AddScoped<UsageQuotaService>();

var app = builder.Build();
app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/ErorPages/NetworkProblem");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;

    headers.TryAdd("X-Content-Type-Options", "nosniff");
    headers.TryAdd("X-Frame-Options", "SAMEORIGIN");
    headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    headers.TryAdd(
        "Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com; " +
        "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com; " +
        "font-src 'self' https://cdnjs.cloudflare.com data:; " +
        "img-src 'self' data: blob:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'self'; " +
        "base-uri 'self'; " +
        "form-action 'self'");

    await next();
});
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<GuestMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();


app.MapControllerRoute(
    name: "Short",
    pattern: "{shortcode:regex(^[[a-zA-Z0-9]]{{6}}$)}",
    defaults: new { controller = "Link", action = "RedirectToOriginal" }
    );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();



app.Run();
