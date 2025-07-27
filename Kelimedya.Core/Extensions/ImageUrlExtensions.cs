using System.Text.RegularExpressions;

namespace Kelimedya.Core.Extensions
{
    public static class ImageUrlExtensions
    {
        public static string ToDriveThumbnail(this string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url ?? string.Empty;
            }

            var match = Regex.Match(url, @"/d/([^/]+)");
            if (match.Success)
            {
                var id = match.Groups[1].Value;
                return $"https://drive.google.com/thumbnail?authuser=0&sz=w320&id={id}";
            }

            return url;
        }
    }
}
