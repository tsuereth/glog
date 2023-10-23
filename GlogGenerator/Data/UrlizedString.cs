using System.Text.RegularExpressions;
using System.Text;

namespace GlogGenerator.Data
{
    public class UrlizedString
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
            return this.urlized.Equals(obj);
        }
    }
}
