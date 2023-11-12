using Markdig.Renderers.Roundtrip;

namespace GlogGenerator.MarkdownExtensions
{
    public class FencedDataBlockRoundtripeRenderer : RoundtripObjectRenderer<FencedDataBlock>
    {
        protected override void Write(RoundtripRenderer renderer, FencedDataBlock obj)
        {
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
        }
    }
}
