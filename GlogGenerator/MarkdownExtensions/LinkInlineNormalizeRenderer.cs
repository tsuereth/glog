using Markdig.Renderers.Normalize;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    // Adapted from Markdig
    public class LinkInlineNormalizeRenderer : NormalizeObjectRenderer<LinkInline>
    {
        protected override void Write(NormalizeRenderer renderer, LinkInline obj)
        {
            if (obj.IsAutoLink && !renderer.Options.ExpandAutoLinks)
            {
                renderer.Write(obj.Url);
                return;
            }

            if (obj.IsImage)
            {
                renderer.Write('!');
            }
            renderer.Write('[');
            renderer.WriteChildren(obj);
            renderer.Write(']');

            if (obj.Label != null)
            {
                if (obj.FirstChild is LiteralInline literal && literal.Content.Length == obj.Label.Length && literal.Content.Match(obj.Label))
                {
                    // collapsed reference and shortcut links
                    if (!obj.IsShortcut)
                    {
                        renderer.Write("[]");
                    }
                }
                else
                {
                    // full link
                    renderer.Write('[').Write(obj.Label).Write(']');
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(obj.Url))
                {
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

            // Bugfix: render any generic attributes that might have been attached.
            var genericAttributes = obj.TryGetReversibleGenericAttributes();
            if (genericAttributes != null)
            {
                renderer.Write(genericAttributes.GetOriginalString());
            }
        }
    }
}
