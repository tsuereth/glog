using System;
using GlogGenerator.Data;
using GlogGenerator.RenderState;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

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
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is HtmlRenderer)
            {
                // Perform variable substitutions before type-specific renderers start rendering.
                renderer.ObjectWriteBefore += new Action<IMarkdownRenderer, MarkdownObject>(
                    (IMarkdownRenderer r, MarkdownObject o) =>
                    {
                        if (o is AutolinkInline)
                        {
                            (o as AutolinkInline).Url = this.variableSubstitution.TryMakeSubstitutions((o as AutolinkInline).Url);
                        }
                        else if (o is LinkInline)
                        {
                            (o as LinkInline).Url = this.variableSubstitution.TryMakeSubstitutions((o as LinkInline).Url);
                        }
                        else if (o is LiteralInline)
                        {
                            // FIXME?: Can (or must) the substitution check preserve un-changed StringSlices?
                            var substitutedString = this.variableSubstitution.TryMakeSubstitutions((o as LiteralInline).Content.ToString());
                            (o as LiteralInline).Content = new StringSlice(substitutedString);
                        }
                    });
            }

            renderer.ObjectRenderers.AddIfNotAlready(new FencedDataBlockRenderer(this.siteDataIndex, this.siteState, this.pageState));
            renderer.ObjectRenderers.AddIfNotAlready<SpoilerRenderer>();
        }
    }
}
