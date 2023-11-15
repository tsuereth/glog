using Markdig.Parsers;
using Markdig.Syntax;

namespace GlogGenerator.MarkdownExtensions
{
    public class TomlFrontMatterBlockParser : FencedBlockParserBase<TomlFrontMatterBlock>
    {
        public TomlFrontMatterBlockParser()
        {
            this.OpeningCharacters = new char[] { '+' };
        }

        protected override TomlFrontMatterBlock CreateFencedBlock(BlockProcessor processor)
        {
            var tomlBlock = new TomlFrontMatterBlock(this);

            if (processor.TrackTrivia)
            {
                tomlBlock.LinesBefore = processor.LinesBefore;
                processor.LinesBefore = null;
                tomlBlock.TriviaBefore = processor.UseTrivia(processor.Start - 1);
                tomlBlock.NewLine = processor.Line.NewLine;
            }

            return tomlBlock;
        }

        public override BlockState TryContinue(BlockProcessor processor, Block block)
        {
            var state = base.TryContinue(processor, block);

            if (state == BlockState.Break || state == BlockState.BreakDiscard)
            {
                var tomlBlock = block as TomlFrontMatterBlock;

                tomlBlock.ParseTomlModel();
            }

            return state;
        }
    }
}
