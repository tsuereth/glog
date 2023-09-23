using System;
using System.Globalization;

namespace GlogGenerator.TemplateRenderers
{
    public class DateTimeRenderer : Antlr4.StringTemplate.DateRenderer
    {
        public override string ToString(object o, string formatString, CultureInfo locale)
        {
            if (string.IsNullOrEmpty(formatString))
            {
                return base.ToString(o, formatString, locale);
            }

            var time = (DateTimeOffset)o;

            if (formatString.Equals("justdate", StringComparison.OrdinalIgnoreCase))
            {
                // Mon, Jan 2, 2006
                var dayOfWeekAbbrev = time.ToString("ddd", CultureInfo.InvariantCulture);
                var monthAbbrev = time.ToString("MMM", CultureInfo.InvariantCulture);
                var dayNum = time.Day;
                var yearNum = time.Year;

                return $"{dayOfWeekAbbrev}, {monthAbbrev} {dayNum}, {yearNum}";
            }
            else if (formatString.Equals("iso8601", StringComparison.OrdinalIgnoreCase))
            {
                // Sat, 02 Feb 2013 16:41:36 -0700
                var dayOfWeekAbbrev = time.ToString("ddd", CultureInfo.InvariantCulture);
                var dayNum = time.ToString("dd", CultureInfo.InvariantCulture);
                var monthAbbrev = time.ToString("MMM", CultureInfo.InvariantCulture);
                var yearNum = time.Year;
                var timeOfDay = time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                var timezoneOffset = time.ToString("zzz", CultureInfo.InvariantCulture).Replace(":", string.Empty);

                return $"{dayOfWeekAbbrev}, {dayNum} {monthAbbrev} {yearNum} {timeOfDay} {timezoneOffset}";
            }

            return base.ToString(o, formatString, locale);
        }
    }
}
