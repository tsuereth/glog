using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GlogGenerator.TemplateRenderers
{
    public class StringRenderer : Antlr4.StringTemplate.StringRenderer
    {
        public static string Urlize(string str)
        {
            str = str.Normalize(NormalizationForm.FormD);
            str = str.Trim().ToLowerInvariant();
            str = Regex.Replace(str, "[^0-9a-z -]", string.Empty);
            str = Regex.Replace(str, "[ -]+", "-");
            str = str.Trim('-');

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

            if (formatString.Equals("urlize", StringComparison.OrdinalIgnoreCase))
            {
                return Urlize(str);
            }

            return base.ToString(o, formatString, culture);
        }
    }
}
