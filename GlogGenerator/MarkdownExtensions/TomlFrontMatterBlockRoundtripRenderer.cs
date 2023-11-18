using System.IO;
using System.Text;
using Markdig.Helpers;
using Markdig.Renderers.Roundtrip;

namespace GlogGenerator.MarkdownExtensions
{
    public class TomlFronMatterBlockRoundtripeRenderer : RoundtripObjectRenderer<TomlFrontMatterBlock>
    {
        protected override void Write(RoundtripRenderer renderer, TomlFrontMatterBlock obj)
        {
            renderer.Write(obj.TriviaBefore);

            renderer.WriteLine("+++");

            var tomlModel = obj.GetModel();

            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                stringWriter.NewLine = "\n";

                tomlModel.WriteTo(stringWriter);
                stringWriter.Flush();
            }

            renderer.Write(stringBuilder.ToString());

            renderer.WriteLine("+++");

            renderer.Write(obj.TriviaAfter);
        }
    }
}
