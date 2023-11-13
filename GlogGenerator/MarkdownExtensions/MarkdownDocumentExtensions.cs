using System.IO;
using Markdig;
using Markdig.Renderers.Roundtrip;
using Markdig.Syntax;

namespace GlogGenerator.MarkdownExtensions
{
    public static class MarkdownDocumentExtensions
    {
        public static string ToMarkdownString(this MarkdownDocument mdDoc, MarkdownPipeline pipeline = null)
        {
            string roundtrip;
            using (var mdTextWriter = new StringWriter())
            {
                var mdRenderer = new RoundtripRenderer(mdTextWriter);
                if (pipeline != null)
                {
                    pipeline.Setup(mdRenderer);
                }

                mdRenderer.Render(mdDoc);
                roundtrip = mdTextWriter.ToString();
            }

            return roundtrip;
        }
    }
}
