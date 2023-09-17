using Markdig.Parsers;

namespace GlogGenerator.MarkdownExtensions
{
    public class QuoteNotSpoilerBlockParser : QuoteBlockParser
    {
        public QuoteNotSpoilerBlockParser() : base() { }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            // If the '>' is followed by '!' then this isn't a quote block after all.
            if (processor.PeekChar(1) == '!')
            {
                return BlockState.None;
            }

            return base.TryOpen(processor);
        }
    }
}
