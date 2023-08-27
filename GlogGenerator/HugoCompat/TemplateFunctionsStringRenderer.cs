using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GlogGenerator.HugoCompat
{
    public class TemplateFunctionsStringRenderer : Antlr4.StringTemplate.StringRenderer
    {
        public static string Urlize(string str, bool htmlEncode, bool terminologySpecial = false)
        {
            // https://github.com/gohugoio/hugo/blob/master/helpers/url_test.go

            str = str.Trim().ToLowerInvariant();

            str = Regex.Replace(str, @"['""’,@\?\!\(\)\[\]\<\>=:]+", string.Empty);

            str = Regex.Replace(str, @"\s+", "-");

            str = Regex.Replace(str, @"[%\-]+", "-");

            if (htmlEncode)
            {
                str = Regex.Replace(str, "&([a-z]+);", "$1");
                str = Uri.EscapeDataString(str);

                // Not this one!
                str = str.Replace("%2F", "/");

                // Weird: '&' was percent-encoded, but we want it gone.
                str = str.Replace("%26", string.Empty);

                // WEIRD: '+' was percent-encoded, but we want it HTML-encoded.
                str = str.Replace("%2B", "&#43;");
            }
            else
            {
                str = Regex.Replace(str, @"[&;]+", string.Empty);

                if (!terminologySpecial)
                {
                    str = str.Replace("+", "-");
                }
            }

            str = Regex.Replace(str, "-+", "-");

            return str;
        }

        public override string ToString(object o, string formatString, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(formatString))
            {
                return base.ToString(o, formatString, culture);
            }

            var str = (string)o;

            if (formatString.Equals("escapepunctuation", StringComparison.OrdinalIgnoreCase))
            {
                str = str.Replace("\"", "&#34;");
                str = str.Replace("'", "&#39;");
                str = str.Replace("+", "&#43;");
                str = str.Replace(">", "&gt;");
                str = str.Replace("<", "&lt;");
                return str;
            }

            if (formatString.Equals("escapeslashes", StringComparison.OrdinalIgnoreCase))
            {
                return str.Replace("/", "\\/");
            }

            if (formatString.Equals("urlize", StringComparison.OrdinalIgnoreCase))
            {
                return Urlize(str, htmlEncode: true);
            }

            return base.ToString(o, formatString, culture);
        }
    }
}
