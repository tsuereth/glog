using Markdig;

namespace GlogGenerator.MarkdownExtensions
{
    public static class MarkdownParserContextExtensions
    {
        const string useSiteDataIndexPropertyName = "glog_useSiteDataIndex";

        public static bool UseSiteDataIndex(this MarkdownParserContext context)
        {
            if (context == null)
            {
                return true;
            }

            if (context.Properties.TryGetValue(useSiteDataIndexPropertyName, out var useSiteDataIndexProperty))
            {
                return (bool)useSiteDataIndexProperty;
            }

            return true;
        }

        public static MarkdownParserContext DontUseSiteDataIndex()
        {
            var context = new MarkdownParserContext();
            context.Properties[useSiteDataIndexPropertyName] = false;
            return context;
        }
    }
}
