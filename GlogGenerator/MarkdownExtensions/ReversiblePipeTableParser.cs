using System.IO;
using System.Linq;
using Markdig.Extensions.Tables;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax;
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

            if (isFinalProcessing)
            {
                var originalSource = state.Block?.TryGetRoundtripOriginalSource();
                if (originalSource == null)
                {
                    return shouldContinue;
                }

                if (state.BlockNew is Table newTable)
                {
                    newTable.SetRoundtripOriginalSource(originalSource);

                    // Bugfix: the built-in postprocessing drops newlines after the original markup.
                    if (state.Block != null)
                    {
                        newTable.NewLine = state.Block.NewLine;
                        newTable.LinesAfter = state.Block.LinesAfter;
                    }
                }
                else if (state.Block is ParagraphBlock { Inline.FirstChild: not null } leadingParagraph)
                {
                    // In some conditions, the table will be seen as an inline sibling
                    // within the current block, not as a new block of its own.
                    // Here the parsed Table won't be IN the PostProcess state yet;
                    // it will have been stashed in a `ProcessInlineDelegate` which,
                    // later during post-post-processing(?), inserts the Table.
                    // (yuck!)
                    // See Markdig's PipeTableParser::PostProcess() for full context:
                    // https://github.com/xoofx/markdig/commit/d6e88f16f7d2d86a096a552250f89415513d09dc
                    var parent = leadingParagraph.Parent!;
                    parent.ProcessInlinesEnd += (_, _) =>
                    {
                        var leadingParagraphIndex = parent.IndexOf(leadingParagraph);
                        if (
                            leadingParagraphIndex != -1 &&
                            parent.Count > (leadingParagraphIndex + 1) &&
                            parent[leadingParagraphIndex + 1] is Table postInsertedTable
                        )
                        {
                            postInsertedTable.SetRoundtripOriginalSource(originalSource);

                            // Bugfix: the built-in postprocessing drops newlines after the original markup.
                            postInsertedTable.NewLine = leadingParagraph.NewLine;
                            postInsertedTable.LinesAfter = leadingParagraph.LinesAfter;

                            // Additionally:
                            // If the "leading paragraph" consists only of a LineBreakInline,
                            // then this whole paragraph is a false parsing artifact of already-handled trivia.
                            // Delete it!!
                            if (
                                leadingParagraph.Inline.Count() == 1 &&
                                leadingParagraph.Inline.FirstChild is LineBreakInline
                            )
                            {
                                leadingParagraph.Inline.Clear();
                                leadingParagraph.NewLine = NewLine.None;
                                leadingParagraph.LinesAfter = null;
                            }
                        }
                    };
                }
            }

            return shouldContinue;
        }
    }
}
