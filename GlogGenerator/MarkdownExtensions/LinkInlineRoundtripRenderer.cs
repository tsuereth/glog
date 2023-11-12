using Markdig.Renderers.Roundtrip;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class LinkInlineRoundtripRenderer : RoundtripObjectRenderer<LinkInline>
    {
        protected override void Write(RoundtripRenderer renderer, LinkInline obj)
        {
            if (obj.IsImage)
            {
                renderer.Write('!');
            }
            // link text
            renderer.Write('[');
            renderer.WriteChildren(obj);
            renderer.Write(']');

            if (obj.Label != null)
            {
                if (obj.LocalLabel == LocalLabel.Local || obj.LocalLabel == LocalLabel.Empty)
                {
                    renderer.Write('[');
                    if (obj.LocalLabel == LocalLabel.Local)
                    {
                        renderer.Write(obj.LabelWithTrivia);
                    }
                    renderer.Write(']');
                }
            }
            else
            {
                if (obj.Url != null)
                {
                    // Bugfix: the built-in roundtrip renderer for LinkInline never actually writes the URL.
                    // This corrected rendering behavior is adapted from the normalize renderer, instead.
                    renderer.Write('(').Write(obj.Url);

                    if (obj.Title is { Length: > 0 })
                    {
                        renderer.Write(" \"");
                        renderer.Write(obj.Title.Replace(@"""", @"\"""));
                        renderer.Write('"');
                    }

                    renderer.Write(')');
                }
            }
        }
    }
}
