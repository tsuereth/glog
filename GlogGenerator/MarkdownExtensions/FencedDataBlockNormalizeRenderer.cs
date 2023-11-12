using Markdig.Helpers;
using Markdig.Renderers.Normalize;

namespace GlogGenerator.MarkdownExtensions
{
    public class FencedDataBlockNormalizeRenderer : NormalizeObjectRenderer<FencedDataBlock>
    {
        protected override void Write(NormalizeRenderer renderer, FencedDataBlock obj)
        {
            renderer.Write(obj.TriviaBefore);

            renderer.Write(":::");
            renderer.WriteLine(obj.Info);
            foreach (var kv in obj.Data)
            {
                renderer.Write(kv.Key);
                renderer.Write(": ");
                renderer.Write(kv.Value.ToString());
                renderer.WriteLine();
            }
            renderer.Write(":::");

            renderer.Write(obj.TriviaAfter);

            if (obj.NewLine != NewLine.None)
            {
                renderer.Write(obj.NewLine.AsString());
            }
        }
    }
}
