using GlogGenerator.Data;
using GlogGenerator.RenderState;
using Markdig;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogMarkdownExtension : IMarkdownExtension
    {
        private readonly SiteDataIndex siteDataIndex;
        private readonly SiteState siteState;
        private readonly PageState pageState;
        private readonly VariableSubstitution variableSubstitution;

        public GlogMarkdownExtension(
            SiteDataIndex siteDataIndex,
            SiteState siteState,
            PageState pageState,
            VariableSubstitution variableSubstitution)
        {
            this.siteDataIndex = siteDataIndex;
            this.siteState = siteState;
            this.pageState = pageState;
            this.variableSubstitution = variableSubstitution;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            // Our custom-links parser can't coexist with the built-in parsers;
            // We need to replace them, and re-use the base parsers as appropriate.
            pipeline.InlineParsers.TryRemove<AutolinkInlineParser>();
            pipeline.InlineParsers.AddIfNotAlready(new GlogAutoLinkInlineParser(this.siteDataIndex, this.siteState));
            pipeline.InlineParsers.TryRemove<LinkInlineParser>();
            pipeline.InlineParsers.AddIfNotAlready(new GlogLinkInlineParser(this.siteDataIndex, this.siteState));

            pipeline.BlockParsers.AddIfNotAlready<FencedDataBlockParser>();

            // The built-in quote block parser will steal '>' at the beginning of a line.
            // We need a customization to not-steal it when followed by '!' for spoilers.
            pipeline.BlockParsers.TryRemove<QuoteBlockParser>();
            pipeline.BlockParsers.AddIfNotAlready<QuoteNotSpoilerBlockParser>();

            pipeline.InlineParsers.AddIfNotAlready<SpoilerParser>();

            pipeline.InlineParsers.AddIfNotAlready<VariableSubstitutionParser>();
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            // We need to replace link renderers, so they can include variable substitutions.
            renderer.ObjectRenderers.TryRemove<AutolinkInlineRenderer>();
            renderer.ObjectRenderers.AddIfNotAlready(new AutolinkInlineWithVariableSubstitutionRenderer(this.variableSubstitution));
            renderer.ObjectRenderers.TryRemove<LinkInlineRenderer>();
            renderer.ObjectRenderers.AddIfNotAlready(new LinkInlineWithVariableSubstitutionRenderer(this.variableSubstitution));

            renderer.ObjectRenderers.AddIfNotAlready(new FencedDataBlockRenderer(this.siteDataIndex, this.siteState, this.pageState));
            renderer.ObjectRenderers.AddIfNotAlready<SpoilerRenderer>();
            renderer.ObjectRenderers.AddIfNotAlready(new VariableSubstitutionRenderer(this.variableSubstitution));
        }
    }
}
