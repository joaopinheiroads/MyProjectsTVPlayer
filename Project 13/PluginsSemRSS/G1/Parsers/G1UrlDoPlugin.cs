using System;
using System.Text.RegularExpressions;

namespace TVPlayerAPI.G1.Parsers
{
    internal static class G1UrlDoPlugin
    {
        private const string HOST = "g1.globo.com";

        private static readonly Regex REGEX_FEED_DYNAMO =
            new Regex(@"^/dynamo/(?:(?<chave>[a-z0-9\-/]+?)/)?rss2\.xml$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex REGEX_FEED_RSS_G1 =
            new Regex(@"^/rss/g1/(?<chave>[a-z0-9\-/]*?)/?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static string ChaveDaSecao(string urlDoPlugin)
        {
            if (!Uri.TryCreate(urlDoPlugin?.Trim(), UriKind.Absolute, out Uri uri)
                || !uri.Host.Equals(HOST, StringComparison.OrdinalIgnoreCase))
                return null;

            Match feed = REGEX_FEED_DYNAMO.Match(uri.AbsolutePath);

            if (!feed.Success)
                feed = REGEX_FEED_RSS_G1.Match(uri.AbsolutePath);

            return feed.Success ? feed.Groups["chave"].Value.Trim('/').ToLowerInvariant() : null;
        }
    }
}
