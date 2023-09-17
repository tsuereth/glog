using System.IO;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace GlogGenerator.MarkdownExtensions
{
    public class SpoilerRenderer : HtmlObjectRenderer<SpoilerInline>
    {
        protected override void Write(HtmlRenderer renderer, SpoilerInline obj)
        {
            if (renderer.EnableHtmlForInline)
            {
                switch (obj.InlineType)
                {
                    case SpoilerInline.SpoilerInlineType.Begin:
                        renderer.Write("<noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">");
                        break;

                    case SpoilerInline.SpoilerInlineType.End:
                        renderer.Write("</spoiler>");
                        break;

                    default:
                        throw new InvalidDataException($"Unrecognized SpoilerInlineType {obj.InlineType}");
                }
            }
        }
    }
}
