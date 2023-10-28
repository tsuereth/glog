using System.Text;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class VariableSubstitutionParser : InlineParser
    {
        public VariableSubstitutionParser()
        {
            this.OpeningCharacters = new char[] { '$' };
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            if (slice.CurrentChar == '$')
            {
                slice.SkipChar();

                var variableNameBuilder = new StringBuilder();
                var c = slice.CurrentChar;
                while (c != '$')
                {
                    if (c == '\0')
                    {
                        return false;
                    }

                    variableNameBuilder.Append(c);
                    c = slice.NextChar();
                }

                if (variableNameBuilder.Length > 0 && c == '$')
                {
                    processor.Inline = new VariableSubstitutionInline(variableNameBuilder.ToString());
                    c = slice.NextChar();
                    return true;
                }
            }

            return false;
        }
    }
}
