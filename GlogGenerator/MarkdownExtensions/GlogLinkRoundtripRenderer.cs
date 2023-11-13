using Markdig.Renderers;
using Markdig.Renderers.Roundtrip;
using Markdig.Syntax.Inlines;
using System.Text;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogLinkRoundtripRenderer : RoundtripObjectRenderer<GlogLinkInline>
    {
        protected override void Write(RoundtripRenderer renderer, GlogLinkInline obj)
        {
            var stringBuilder = new StringBuilder();
            if (obj.IsAutoLink)
            {
                stringBuilder.Append('<');
                stringBuilder.Append(obj.ReferenceType);
                stringBuilder.Append(':');
                stringBuilder.Append(obj.ReferenceKey);
                stringBuilder.Append('>');
            }
            else
            {
                renderer.Write('[');
                renderer.WriteChildren(obj);

                stringBuilder.Append("](");
                stringBuilder.Append(obj.ReferenceType);
                stringBuilder.Append(':');
                stringBuilder.Append(obj.ReferenceKey);
                stringBuilder.Append(')');
            }

            renderer.Write(stringBuilder.ToString());
        }
    }
}
