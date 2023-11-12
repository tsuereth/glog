using Markdig.Helpers;

namespace GlogGenerator.MarkdownExtensions
{
    public class RoundtripOriginalSource
    {
        private string originalString;

        public RoundtripOriginalSource(StringSlice slice)
        {
            this.originalString = slice.ToString();
        }

        public string GetOriginalString()
        {
            return this.originalString;
        }
    }
}
