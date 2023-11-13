using System.IO;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;

namespace GlogGenerator.MarkdownExtensions
{
    public class SpoilerNormalizeRenderer : NormalizeObjectRenderer<SpoilerInline>
    {
        protected override void Write(NormalizeRenderer renderer, SpoilerInline obj)
        {
            switch (obj.InlineType)
            {
                case SpoilerInline.SpoilerInlineType.Begin:
                    renderer.Write(">!");
                    break;

                case SpoilerInline.SpoilerInlineType.End:
                    renderer.Write("!<");
                    break;

                default:
                    throw new InvalidDataException($"Unrecognized SpoilerInlineType {obj.InlineType}");
            }
        }
    }
}
