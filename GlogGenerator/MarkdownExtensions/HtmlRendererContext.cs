namespace GlogGenerator.MarkdownExtensions
{
    public class HtmlRendererContext
    {
        private string pageHashCode;

        public void Clear()
        {
            this.pageHashCode = null;
        }

        public string GetPageHashCode()
        {
            return this.pageHashCode;
        }

        public void SetPageHashCode(string pageHashCode)
        {
            this.pageHashCode = pageHashCode;
        }
    }
}
