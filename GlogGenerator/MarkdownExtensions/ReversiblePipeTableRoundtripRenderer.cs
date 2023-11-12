using Markdig.Extensions.Tables;
using Markdig.Renderers.Roundtrip;

namespace GlogGenerator.MarkdownExtensions
{
    public class ReversibleTableRoundtripRenderer : RoundtripObjectRenderer<Table>
    {
        protected override void Write(RoundtripRenderer renderer, Table obj)
        {
            var originalSource = obj.TryGetRoundtripOriginalSource();
            if (originalSource != null)
            {
                renderer.Write(originalSource.GetOriginalString());
            }
        }
    }
}
