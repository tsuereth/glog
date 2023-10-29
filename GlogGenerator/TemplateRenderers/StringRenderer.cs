using System;
using System.Globalization;
using GlogGenerator.Data;

namespace GlogGenerator.TemplateRenderers
{
    public class StringRenderer : Antlr4.StringTemplate.StringRenderer
    {
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
                return UrlizedString.Urlize(str);
            }

            return base.ToString(o, formatString, culture);
        }
    }
}
