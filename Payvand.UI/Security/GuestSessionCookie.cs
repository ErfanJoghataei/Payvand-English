using System.Security.Cryptography;
using System.Text;

namespace Payvand.UI.Security;

public static class GuestSessionCookie
{
    public const string Name = "__Host-Payvand.Guest";

    public static string CreateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public static string? GetHash(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue(Name, out var token) ||
            token.Length is < 40 or > 64)
        {
            return null;
        }

        return Hash(token);
    }

    public static CookieOptions CreateOptions(DateTimeOffset? expires = null) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Path = "/",
        IsEssential = true,
        Expires = expires ?? DateTimeOffset.UtcNow.AddDays(30)
    };
}
