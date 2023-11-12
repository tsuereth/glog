using Markdig.Extensions.Tables;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class ReversiblePipeTableParser : PipeTableParser, IPostInlineProcessor
    {
        public ReversiblePipeTableParser(LineBreakInlineParser lineBreakParser, PipeTableOptions options = null)
            : base(lineBreakParser, options) { }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var originalSlice = slice;
            var matched = base.Match(processor, ref slice);
            if (matched && processor.Block != null)
            {
                var alreadySetOriginalSource = (processor.Block.TryGetRoundtripOriginalSource() != null);
                if (!alreadySetOriginalSource)
                {
                    processor.Block.SetRoundtripOriginalSource(new RoundtripOriginalSource(originalSlice));
                }
            }

            return matched;
        }

        public new bool PostProcess(InlineProcessor state, Inline root, Inline lastChild, int postInlineProcessorIndex, bool isFinalProcessing)
        {
            var shouldContinue = base.PostProcess(state, root, lastChild, postInlineProcessorIndex, isFinalProcessing);

            if (isFinalProcessing && state.BlockNew is Table table)
            {
                var originalSource = state.Block?.TryGetRoundtripOriginalSource();
                table.SetRoundtripOriginalSource(originalSource);
            }

            return shouldContinue;
        }
    }
}
