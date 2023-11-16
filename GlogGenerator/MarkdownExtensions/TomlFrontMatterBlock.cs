using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Tomlyn;
using Tomlyn.Model;

namespace GlogGenerator.MarkdownExtensions
{
    public class TomlFrontMatterBlock : LeafBlock, IFencedBlock
    {
        public char FencedChar { get; set; }

        public int OpeningFencedCharCount { get; set; }

        public StringSlice TriviaAfterFencedChar { get; set; }

        public string Info { get; set; }

        public StringSlice UnescapedInfo { get; set; }

        public StringSlice TriviaAfterInfo { get; set; }

        public string Arguments { get; set; }

        public StringSlice UnescapedArguments { get; set; }

        public StringSlice TriviaAfterArguments { get; set; }

        public NewLine InfoNewLine { get; set; }

        public StringSlice TriviaBeforeClosingFence { get; set; }

        public int ClosingFencedCharCount { get; set; }

        public TomlFrontMatterBlock(BlockParser parser) : base(parser)
        {
        }

        public TomlTable Model { get; private set; } = null;

        public void ParseTomlModel()
        {
            var tomlText = string.Join('\n', this.Lines.Lines);

            this.Model = Toml.ToModel(tomlText);
        }
    }
}
