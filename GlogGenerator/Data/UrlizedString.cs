using System;
using System.Text;
using System.Text.RegularExpressions;

namespace GlogGenerator.Data
{
    public class UrlizedString : IEquatable<UrlizedString>
    {
        public static string Urlize(string str)
        {
            str = str.Normalize(NormalizationForm.FormD);
            str = str.Trim().ToLowerInvariant();
            str = Regex.Replace(str, @"[^0-9a-z \-\+]", string.Empty);
            str = Regex.Replace(str, @"[ \-]+", "-");
            str = str.Trim('-');

            return str;
        }

        private readonly string urlized;

        public UrlizedString(string str)
        {
            this.urlized = Urlize(str);
        }

        public override int GetHashCode()
        {
            return this.urlized.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.urlized.Equals(obj as UrlizedString);
        }

        public bool Equals(UrlizedString other)
        {
            return this.urlized.Equals(other.urlized, StringComparison.Ordinal);
        }
    }
}
