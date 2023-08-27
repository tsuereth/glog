using Markdig;
using Markdig.Renderers;

namespace GlogGenerator.HugoCompat
{
    public class MarkdownQuirksMarkdigExtension : IMarkdownExtension
    {
        // https://github.com/xoofx/markdig/blob/master/src/Markdig/MarkdownPipelineBuilder.cs

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is HtmlRenderer htmlRenderer)
            {
                // https://github.com/xoofx/markdig/blob/master/src/Markdig/Renderers/HtmlRenderer.cs
                //htmlRenderer.EnableHtmlForInline = false;
                //htmlRenderer.EnableHtmlForBlock = false;
                //htmlRenderer.ImplicitParagraph = false;
                //htmlRenderer.UseNonAsciiNoEscape = false;
                htmlRenderer.EnableHtmlEscape = true;
            }
        }
    }
}
