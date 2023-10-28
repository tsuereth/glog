using System.Collections.Generic;
using System.IO;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class LinkInlineWithVariableSubstitutionRenderer : LinkInlineRenderer
    {
        private readonly VariableSubstitution variableSubstitution;

        public LinkInlineWithVariableSubstitutionRenderer(
            VariableSubstitution variableSubstitution)
            : base()
        {
            this.variableSubstitution = variableSubstitution;
        }

        protected override void Write(HtmlRenderer renderer, LinkInline link)
        {
            link.Url = this.variableSubstitution.TryMakeSubstitutions(link.Url);

            base.Write(renderer, link);
        }
    }
}
