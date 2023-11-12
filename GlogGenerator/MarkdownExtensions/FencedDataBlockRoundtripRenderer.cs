using Markdig.Helpers;
using Markdig.Renderers.Roundtrip;

namespace GlogGenerator.MarkdownExtensions
{
    public class FencedDataBlockRoundtripeRenderer : RoundtripObjectRenderer<FencedDataBlock>
    {
        protected override void Write(RoundtripRenderer renderer, FencedDataBlock obj)
        {
            renderer.RenderLinesBefore(obj);

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

            renderer.RenderLinesAfter(obj);
        }
    }
}
