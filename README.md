# Payvand (پیوند)

## English copy

This folder is a separate English edition of the project. Its connection string points to the local `PayvandEnglishDb` database, leaving the original project and database alone. English Razor views, validation messages, legal pages, SEO text, and five seeded journal articles are included.

The `docs/` directory contains a public static preview for GitHub Pages: the home page, journal articles, legal pages, and a dashboard reached from the home page without login. Dashboard navigation works with labeled sample data; server actions are disabled in the preview. The full application requires ASP.NET Core and a database.

To run locally, use .NET 10 and SQL Server LocalDB:

```powershell
dotnet ef database update --project Payvand.DAL --startup-project Payvand.UI
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project Payvand.UI
```

Set external service credentials in user secrets or environment variables if you want to test Google sign in, SMS, or Safe Browsing. The local copy runs with those integrations disabled when credentials are absent.

Payvand is a personal ASP.NET Core MVC project for shortening links, generating QR codes, and reviewing link activity from a dashboard.

Live site: [payvnd.ir](https://payvnd.ir)

## Implemented capabilities

- Short-link creation and redirect handling
- Guest-session flows, with account authentication temporarily disabled
- QR-code generation for managed links
- Click logging and dashboard statistics
- OTP-based account flows
- Safe Browsing checks and violation reporting
- SEO metadata, terms, privacy, and error pages

## Stack and structure

- ASP.NET Core MVC on .NET 10
- Entity Framework Core 10 and SQL Server
- Razor views, JavaScript, CSS, and Bootstrap
- QRCoder/ImageSharp for generated QR assets
- Kavenegar for OTP delivery
- Serilog for application logging

```text
Payvand.UI/                 MVC controllers, views, middleware, and static assets
Payvand.BusinessLogic/      Link, QR, OTP, and safety-related services
Payvand.DAL/                EF Core context, entities, and migrations
```

## Local setup

Requirements: .NET 10 SDK and SQL Server or LocalDB.

Copy `Payvand.UI/appsettings.example.json` to `Payvand.UI/appsettings.json`, then provide local values through .NET User Secrets or environment variables:

```text
ConnectionStrings__cnnstring
Kavenegar__Sender
Kavenegar__receptor
Kavenegar__api
GoogleSafeBrowsing__Key
```

```bash
dotnet restore Payvand.slnx
dotnet run --project Payvand.UI/Payvand.UI.csproj
```

## Security note

Real API keys, database credentials, phone numbers, and production settings must not be committed. The tracked example contains placeholders only.

## Project status

Payvand is an actively developed portfolio project. The live service may differ from the public documentation while features are being tested and deployed.

## Optional Google sign-in

OAuth credentials and API keys must not be committed to `appsettings.json`. Configure local values with .NET user-secrets:

```powershell
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_CLIENT_ID" --project Payvand.UI
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_CLIENT_SECRET" --project Payvand.UI
dotnet user-secrets set "GoogleSafeBrowsing:Key" "YOUR_SAFE_BROWSING_KEY" --project Payvand.UI
```

Create a Google OAuth 2.0 Web client and register both the local and production callback URLs:

- `https://localhost:<https-port>/signin-google`
- `https://your-domain.example/signin-google`

For production, use environment variables such as `Authentication__Google__ClientId` and
`Authentication__Google__ClientSecret`. Also set `AllowedHosts` to the exact production
hostname (or a semicolon-separated list of trusted hostnames).

Authentication is currently disabled by default through `Authentication__Enabled=false`.
