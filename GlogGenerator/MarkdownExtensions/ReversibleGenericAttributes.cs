namespace GlogGenerator.MarkdownExtensions
{
    public class ReversibleGenericAttributes
    {
        private string originalString;

        public ReversibleGenericAttributes(string originalString)
        {
            this.originalString = originalString;
        }

        public string GetOriginalString()
        {
            return this.originalString;
        }
    }
}
