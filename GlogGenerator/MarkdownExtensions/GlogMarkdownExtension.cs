using GlogGenerator.RenderState;
using Markdig;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogMarkdownExtension : IMarkdownExtension
    {
        private readonly SiteState siteState;
        private readonly PageState pageState;

        public GlogMarkdownExtension(
            SiteState siteState,
            PageState pageState)
        {
            this.siteState = siteState;
            this.pageState = pageState;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            // Our custom-links parser can't coexist with the built-in parsers;
            // We need to replace them, and re-use the base parsers as appropriate.
            pipeline.InlineParsers.TryRemove<AutolinkInlineParser>();
            pipeline.InlineParsers.AddIfNotAlready(new GlogAutoLinkInlineParser(this.siteState));
            pipeline.InlineParsers.TryRemove<LinkInlineParser>();
            pipeline.InlineParsers.AddIfNotAlready(new GlogLinkInlineParser(this.siteState));

            pipeline.BlockParsers.AddIfNotAlready<FencedDataBlockParser>();

            // The built-in quote block parser will steal '>' at the beginning of a line.
            // We need a customization to not-steal it when followed by '!' for spoilers.
            pipeline.BlockParsers.TryRemove<QuoteBlockParser>();
            pipeline.BlockParsers.AddIfNotAlready<QuoteNotSpoilerBlockParser>();

            pipeline.InlineParsers.AddIfNotAlready<SpoilerParser>();
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.AddIfNotAlready(new FencedDataBlockRenderer(this.siteState, this.pageState));
            renderer.ObjectRenderers.AddIfNotAlready<SpoilerRenderer>();
        }
    }
}
