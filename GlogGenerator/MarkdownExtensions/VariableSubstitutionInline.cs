using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class VariableSubstitutionInline : LeafInline
    {
        public string VariableName { get; set; }

        public VariableSubstitutionInline(string variableName)
        {
            this.VariableName = variableName;
        }
    }
}
