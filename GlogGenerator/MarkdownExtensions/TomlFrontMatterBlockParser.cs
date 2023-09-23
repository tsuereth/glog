using Markdig.Parsers;

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
            return new TomlFrontMatterBlock(this);
        }
    }
}
