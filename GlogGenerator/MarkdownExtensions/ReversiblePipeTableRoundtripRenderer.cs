using Markdig.Extensions.Tables;
using Markdig.Helpers;
using Markdig.Renderers.Roundtrip;

namespace GlogGenerator.MarkdownExtensions
{
    public class ReversibleTableRoundtripRenderer : RoundtripObjectRenderer<Table>
    {
        protected override void Write(RoundtripRenderer renderer, Table obj)
        {
            renderer.RenderLinesBefore(obj);

            renderer.Write(obj.TriviaBefore);

            var originalSource = obj.TryGetRoundtripOriginalSource();
            if (originalSource != null)
            {
                renderer.Write(originalSource.GetOriginalString());
            }

            renderer.Write(obj.TriviaAfter);

            if (obj.NewLine != NewLine.None)
            {
                renderer.Write(obj.NewLine.AsString());
            }

            renderer.RenderLinesAfter(obj);
        }
    }
}
