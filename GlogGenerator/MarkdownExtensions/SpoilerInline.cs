using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class SpoilerInline : LeafInline
    {
        public SpoilerInlineType InlineType { get; set; }

        public static SpoilerInline BeginSpoiler()
        {
            var begin = new SpoilerInline()
            {
                InlineType = SpoilerInlineType.Begin,
            };

            return begin;
        }

        public static SpoilerInline EndSpoiler()
        {
            var end = new SpoilerInline()
            {
                InlineType = SpoilerInlineType.End,
            };

            return end;
        }

        public enum SpoilerInlineType
        {
            Begin,
            End,
        }
    }
}
