using System;
using System.Globalization;
using GlogGenerator.Data;

namespace GlogGenerator
{
	public static class ScribanTemplateFunctions
    {
        public static string DatetimeIso8601(DateTimeOffset inDatetime)
        {
            // Sat, 02 Feb 2013 16:41:36 -0700
            var dayOfWeekAbbrev = inDatetime.ToString("ddd", CultureInfo.InvariantCulture);
            var dayNum = inDatetime.ToString("dd", CultureInfo.InvariantCulture);
            var monthAbbrev = inDatetime.ToString("MMM", CultureInfo.InvariantCulture);
            var yearNum = inDatetime.Year;
            var timeOfDay = inDatetime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var timezoneOffset = inDatetime.ToString("zzz", CultureInfo.InvariantCulture).Replace(":", string.Empty);

            return $"{dayOfWeekAbbrev}, {dayNum} {monthAbbrev} {yearNum} {timeOfDay} {timezoneOffset}";
        }

        public static string DatetimeJustDate(DateTimeOffset inDatetime)
        {
            // Mon, Jan 2, 2006
            var dayOfWeekAbbrev = inDatetime.ToString("ddd", CultureInfo.InvariantCulture);
            var monthAbbrev = inDatetime.ToString("MMM", CultureInfo.InvariantCulture);
            var dayNum = inDatetime.Day;
            var yearNum = inDatetime.Year;

            return $"{dayOfWeekAbbrev}, {monthAbbrev} {dayNum}, {yearNum}";
        }

        public static string DatetimeJustYear(DateTimeOffset inDatetime)
        {
            return inDatetime.Year.ToString(CultureInfo.InvariantCulture);
        }

        public static string StringEscapeForRssxml(string inString)
        {
            if (string.IsNullOrEmpty(inString))
            {
                return string.Empty;
            }

            return inString
                .Trim()
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&#34;")
                .Replace("'", "&#39;");
        }

        public static string StringEscapePunctuation(string inString)
        {
            if (string.IsNullOrEmpty(inString))
            {
                return string.Empty;
            }

            return inString
                .Replace("\"", "&#34;")
                .Replace("'", "&#39;")
                .Replace("+", "&#43;")
                .Replace(">", "&gt;")
                .Replace("<", "&lt;");
        }

        public static string StringUrlize(string inString)
        {
            return UrlizedString.Urlize(inString);
        }
    }
}
