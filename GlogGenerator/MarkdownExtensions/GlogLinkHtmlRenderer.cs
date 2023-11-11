using GlogGenerator.Data;
using GlogGenerator.RenderState;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogLinkHtmlRenderer : HtmlObjectRenderer<GlogLinkInline>
    {
        private readonly LinkInlineRenderer linkInlineRenderer;
        private readonly ISiteDataIndex siteDataIndex;
        private readonly SiteState siteState;

        public GlogLinkHtmlRenderer(
            LinkInlineRenderer linkInlineRenderer,
            ISiteDataIndex siteDataIndex,
            SiteState siteState)
        {
            this.linkInlineRenderer = linkInlineRenderer;
            this.siteDataIndex = siteDataIndex;
            this.siteState = siteState;
        }

        protected override void Write(HtmlRenderer renderer, GlogLinkInline obj)
        {
            var linkHandler = GlogLinkHandlers.LinkMatchHandlers[obj.ReferenceType];

            var link = obj as LinkInline;
            link.Url = linkHandler(this.siteDataIndex, this.siteState, obj.ReferenceKey);

            this.linkInlineRenderer.Write(renderer, link);
        }
    }
}
