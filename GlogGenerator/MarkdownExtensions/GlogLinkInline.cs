using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogLinkInline : LinkInline
    {
        public string ReferenceType { get; set; }

        public string ReferenceKey { get; set; }
    }
}
