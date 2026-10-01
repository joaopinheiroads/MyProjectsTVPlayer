using System;
using System.Globalization;

namespace TVPlayerAPI.Helpers
{
    internal static class DateTimeHelper
    {
        private const string FORMATO_PUBDATE_RFC822 = "ddd, dd MMM yyyy HH:mm:ss";

        private const string FUSO_NUMERICO_UTC = " +0000";

        internal static DateTime UnixTimestampToDateTime(double unixTime)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return unixStart.AddSeconds(unixTime);
        }

        internal static string PubDateRssEmUtc(DateTime quando)
        {
            DateTime utc = quando.Kind == DateTimeKind.Local
                ? quando.ToUniversalTime()
                : DateTime.SpecifyKind(quando, DateTimeKind.Utc);

            return utc.ToString(FORMATO_PUBDATE_RFC822, CultureInfo.InvariantCulture) + FUSO_NUMERICO_UTC;
        }
    }
}
