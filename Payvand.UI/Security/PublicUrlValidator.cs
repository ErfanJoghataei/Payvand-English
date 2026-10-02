using System.Net;

namespace Payvand.UI.Security;

public static class PublicUrlValidator
{
    public static bool IsAllowed(Uri uri, string currentHost)
    {
        if (!uri.IsAbsoluteUri ||
            (!uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
             !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) ||
            string.IsNullOrWhiteSpace(uri.Host) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            uri.AbsoluteUri.Length > 2048)
        {
            return false;
        }

        var host = uri.IdnHost.TrimEnd('.');
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            !host.Contains('.') ||
            IPAddress.TryParse(host, out _) ||
            host.Equals(currentHost.TrimEnd('.'), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
