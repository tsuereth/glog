using System;
using GlogGenerator.Data;
using GlogGenerator.RenderState;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogMarkdownExtension : IMarkdownExtension
    {
        private readonly SiteBuilder siteBuilder;
        private readonly SiteDataIndex siteDataIndex;
        private readonly SiteState siteState;

        private HtmlRendererContext htmlRendererContext;

        public GlogMarkdownExtension(
            SiteBuilder siteBuilder,
            SiteDataIndex siteDataIndex,
            SiteState siteState)
        {
            this.siteBuilder = siteBuilder;
            this.siteDataIndex = siteDataIndex;
            this.siteState = siteState;

            this.htmlRendererContext = new HtmlRendererContext();
        }

        public HtmlRendererContext GetRendererContext()
        {
            return this.htmlRendererContext;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            // Our custom-links parser can't coexist with the built-in parsers;
            // We need to replace them, and re-use the base parsers as appropriate.
            pipeline.InlineParsers.TryRemove<AutolinkInlineParser>();
            pipeline.InlineParsers.AddIfNotAlready<GlogAutoLinkInlineParser>();
            pipeline.InlineParsers.TryRemove<LinkInlineParser>();
            pipeline.InlineParsers.AddIfNotAlready<GlogLinkInlineParser>();

            pipeline.BlockParsers.AddIfNotAlready<FencedDataBlockParser>();
            pipeline.BlockParsers.AddIfNotAlready<TomlFrontMatterBlockParser>();

            // The built-in quote block parser will steal '>' at the beginning of a line.
            // We need a customization to not-steal it when followed by '!' for spoilers.
            pipeline.BlockParsers.TryRemove<QuoteBlockParser>();
            pipeline.BlockParsers.AddIfNotAlready<QuoteNotSpoilerBlockParser>();

            pipeline.InlineParsers.AddIfNotAlready<SpoilerParser>();
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is HtmlRenderer)
            {
                // Perform variable substitutions before type-specific renderers start rendering.
                renderer.ObjectWriteBefore += new Action<IMarkdownRenderer, MarkdownObject>(
                    (IMarkdownRenderer r, MarkdownObject o) =>
                    {
                        var vs = this.siteBuilder.GetVariableSubstitution();

                        if (o is AutolinkInline)
                        {
                            (o as AutolinkInline).Url = vs.TryMakeSubstitutions((o as AutolinkInline).Url);
                        }
                        else if (o is LinkInline)
                        {
                            (o as LinkInline).Url = vs.TryMakeSubstitutions((o as LinkInline).Url);
                        }
                        else if (o is LiteralInline)
                        {
                            // FIXME?: Can (or must) the substitution check preserve un-changed StringSlices?
                            var substitutedString = vs.TryMakeSubstitutions((o as LiteralInline).Content.ToString());
                            (o as LiteralInline).Content = new StringSlice(substitutedString);
                        }
                    });
            }

            renderer.ObjectRenderers.InsertBefore<LinkInlineRenderer>(
                new GlogLinkInlineRenderer(
                    renderer.ObjectRenderers.Find<LinkInlineRenderer>(),
                    this.siteDataIndex,
                    this.siteState));

            renderer.ObjectRenderers.AddIfNotAlready(new FencedDataBlockRenderer(this.siteDataIndex, this.htmlRendererContext));
            renderer.ObjectRenderers.AddIfNotAlready<SpoilerRenderer>();
        }
    }
}
