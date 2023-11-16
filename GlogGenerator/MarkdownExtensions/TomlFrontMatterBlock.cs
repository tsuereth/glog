using System.Text;
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

        private StringBuilder tomlSourceLines = new StringBuilder();

        private TomlTable tomlModel = null;

        public TomlFrontMatterBlock(BlockParser parser) : base(parser)
        {
        }

        public void AccumulateParsedLine(StringSlice parsedLine)
        {
            // QUIRK NOTE: If a Model was previously built, wipe it out.
            if (tomlModel != null)
            {
                tomlModel = null;
            }

            var lineText = parsedLine.Text.Substring(parsedLine.Start, parsedLine.Length);
            this.tomlSourceLines.AppendLine(lineText);
        }

        public TomlTable GetModel()
        {
            if (this.tomlModel == null)
            {
                var tomlSourceString = this.tomlSourceLines.ToString();
                this.tomlModel = Toml.ToModel(tomlSourceString);
            }

            return this.tomlModel;
        }
    }
}
