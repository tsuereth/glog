using System.Collections.Generic;
using System.IO;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace GlogGenerator.MarkdownExtensions
{
    public class VariableSubstitutionRenderer : HtmlObjectRenderer<VariableSubstitutionInline>
    {
        private readonly VariableSubstitution variableSubstitution;

        public VariableSubstitutionRenderer(
            VariableSubstitution variableSubstitution)
        {
            this.variableSubstitution = variableSubstitution;
        }

        protected override void Write(HtmlRenderer renderer, VariableSubstitutionInline obj)
        {
            if (!this.variableSubstitution.TryGetSubstitution(obj.VariableName, out var substitionValue))
            {
                throw new InvalidDataException($"Unrecognized variable name {obj.VariableName}");
            }

            renderer.Write(substitionValue);
        }
    }
}
