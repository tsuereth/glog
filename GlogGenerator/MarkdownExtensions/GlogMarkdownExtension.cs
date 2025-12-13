using System;
using GlogGenerator.Data;
using GlogGenerator.RenderState;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using Markdig.Renderers.Roundtrip;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogMarkdownExtension : IMarkdownExtension
    {
        private readonly VariableSubstitution variableSubstitution;
        private readonly ISiteDataIndex siteDataIndex;
        private readonly SiteState siteState;

        private HtmlRendererContext htmlRendererContext;

        public GlogMarkdownExtension(
            VariableSubstitution variableSubstitution,
            ISiteDataIndex siteDataIndex,
            SiteState siteState)
        {
            this.variableSubstitution = variableSubstitution;
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
            pipeline.InlineParsers.Insert(0, new ReversibleGenericAttributesParser());

            pipeline.InlineParsers.InsertBefore<AutolinkInlineParser>(new GlogAutoLinkInlineParser(this.siteDataIndex));
            pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new GlogLinkInlineParser(this.siteDataIndex));

            pipeline.BlockParsers.AddIfNotAlready<FencedDataBlockParser>();
            pipeline.BlockParsers.AddIfNotAlready<TomlFrontMatterBlockParser>();

            // Markdig offers an extension for "extra" list item bullet-type parsing,
            // but it doesn't work totally right for roundtrip rendering, so use a custom parser.
            var listParser = pipeline.BlockParsers.FindExact<ListBlockParser>();
            listParser.ItemParsers.AddIfNotAlready<ListExtraItemBulletParser>();

            // The built-in quote block parser will steal '>' at the beginning of a line.
            // We need a customization to not-steal it when followed by '!' for spoilers.
            pipeline.BlockParsers.TryRemove<QuoteBlockParser>();
            pipeline.BlockParsers.AddIfNotAlready<QuoteNotSpoilerBlockParser>();

            // Replace (wrap) the built-in table parser, so we can preserve its original source for roundtrip rendering.
            pipeline.InlineParsers.Replace<PipeTableParser>(
                new ReversiblePipeTableParser(
                    pipeline.InlineParsers.FindExact<LineBreakInlineParser>(),
                    new PipeTableOptions()));

            pipeline.InlineParsers.AddIfNotAlready<SpoilerParser>();

            // Plug the generic attribute parser into all IAttributesParseables
            foreach (var parser in pipeline.BlockParsers)
            {
                if (parser is IAttributesParseable attributesParseable)
                {
                    attributesParseable.TryParseAttributes = ReversibleGenericAttributesParser.TryProcessAttributesForHeading;
                }
            }
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

                renderer.ObjectRenderers.InsertBefore<Markdig.Renderers.Html.Inlines.LinkInlineRenderer>(
                    new GlogLinkHtmlRenderer(
                        renderer.ObjectRenderers.Find<Markdig.Renderers.Html.Inlines.LinkInlineRenderer>(),
                        this.siteDataIndex,
                        this.siteState));

                renderer.ObjectRenderers.AddIfNotAlready(new FencedDataBlockHtmlRenderer(this.siteDataIndex, this.htmlRendererContext));
                renderer.ObjectRenderers.AddIfNotAlready<SpoilerHtmlRenderer>();
            }
            else if (renderer is NormalizeRenderer)
            {
                renderer.ObjectRenderers.InsertBefore<Markdig.Renderers.Normalize.Inlines.AutolinkInlineRenderer>(new GlogLinkNormalizeRenderer(this.siteDataIndex));
                renderer.ObjectRenderers.AddIfNotAlready<FencedDataBlockNormalizeRenderer>();
                renderer.ObjectRenderers.AddIfNotAlready<SpoilerNormalizeRenderer>();

                // The built-in normalize renderer is missing generic attributes, so, replace it.
                renderer.ObjectRenderers.Replace<Markdig.Renderers.Normalize.Inlines.LinkInlineRenderer>(new LinkInlineNormalizeRenderer());
            }
            else if (renderer is RoundtripRenderer)
            {
                renderer.ObjectRenderers.InsertBefore<Markdig.Renderers.Roundtrip.Inlines.AutolinkInlineRenderer>(new GlogLinkRoundtripRenderer(this.siteDataIndex));
                renderer.ObjectRenderers.AddIfNotAlready<FencedDataBlockRoundtripeRenderer>();
                renderer.ObjectRenderers.AddIfNotAlready<SpoilerRoundtripRenderer>();

                // The built-in roundtrip renderer for LinkInline doesn't work! so, replace it.
                renderer.ObjectRenderers.Replace<Markdig.Renderers.Roundtrip.Inlines.LinkInlineRenderer>(new LinkInlineRoundtripRenderer());

                // Built-in table handling doesn't have a roundtrip renderer.
                renderer.ObjectRenderers.AddIfNotAlready<ReversibleTableRoundtripRenderer>();

                renderer.ObjectRenderers.AddIfNotAlready<TomlFronMatterBlockRoundtripeRenderer>();
            }
        }
    }
}
