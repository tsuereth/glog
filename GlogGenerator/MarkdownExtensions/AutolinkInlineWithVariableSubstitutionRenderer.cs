using System.Collections.Generic;
using System.IO;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class AutolinkInlineWithVariableSubstitutionRenderer : AutolinkInlineRenderer
    {
        private readonly VariableSubstitution variableSubstitution;

        public AutolinkInlineWithVariableSubstitutionRenderer(
            VariableSubstitution variableSubstitution)
            : base()
        {
            this.variableSubstitution = variableSubstitution;
        }

        protected override void Write(HtmlRenderer renderer, AutolinkInline obj)
        {
            obj.Url = this.variableSubstitution.TryMakeSubstitutions(obj.Url);

            base.Write(renderer, obj);
        }
    }
}
