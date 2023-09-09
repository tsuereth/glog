using GlogGenerator.RenderState;
using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogMarkdownExtension : IMarkdownExtension
    {
        private readonly SiteState siteState;

        public GlogMarkdownExtension(
            SiteState siteState)
        {
            this.siteState = siteState;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            // Our custom-links parser can't coexist with the built-in parsers;
            // We need to replace them, and re-use the base parsers as appropriate.
            pipeline.InlineParsers.TryRemove<AutolinkInlineParser>();
            pipeline.InlineParsers.AddIfNotAlready(new GlogAutoLinkInlineParser(this.siteState));
            pipeline.InlineParsers.TryRemove<LinkInlineParser>();
            pipeline.InlineParsers.AddIfNotAlready(new GlogLinkInlineParser(this.siteState));
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            // No-op, existing renderers will work just fine.

            //renderer.ObjectRenderers.AddIfNotAlready<GlogLinkRenderer>();
        }
    }
}
