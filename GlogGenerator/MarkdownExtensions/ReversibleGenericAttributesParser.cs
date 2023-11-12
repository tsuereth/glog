using Markdig.Extensions.GenericAttributes;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace GlogGenerator.MarkdownExtensions
{
    public class ReversibleGenericAttributesParser : GenericAttributesParser
    {
        public ReversibleGenericAttributesParser() : base() { }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var attrStartPos = slice.Start;
            var matched = base.Match(processor, ref slice);
            if (matched)
            {
                var attrEndPos = slice.Start;
                var attrString = slice.Text.Substring(attrStartPos, attrEndPos - attrStartPos);
                var attr = new ReversibleGenericAttributes(attrString);

                // TODO?: is this always the right element?
                processor.Inline.SetReversibleGenericAttributes(attr);
            }

            return matched;
        }

        // Extracted from Markdig's GenericAttributesExtension
        public static bool TryProcessAttributesForHeading(BlockProcessor processor, ref StringSlice line, IBlock block)
        {
            // Try to find if there is any attributes { in the info string on the first line of a FencedCodeBlock
            if (line.Start < line.End)
            {
                int indexOfAttributes = line.IndexOf('{');
                if (indexOfAttributes >= 0)
                {
                    // Work on a copy
                    var copy = line;
                    copy.Start = indexOfAttributes;
                    var startOfAttributes = copy.Start;
                    if (GenericAttributesParser.TryParse(ref copy, out HtmlAttributes attributes))
                    {
                        var htmlAttributes = block.GetAttributes();
                        attributes.CopyTo(htmlAttributes);

                        // Update position for HtmlAttributes
                        htmlAttributes.Line = processor.LineIndex;
                        htmlAttributes.Column = startOfAttributes - processor.CurrentLineStartPosition; // This is not accurate with tabs!
                        htmlAttributes.Span.Start = startOfAttributes;
                        htmlAttributes.Span.End = copy.Start - 1;

                        line.End = indexOfAttributes - 1;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
