﻿using Markdig.Helpers;
using Markdig.Renderers.Roundtrip;

namespace GlogGenerator.MarkdownExtensions
{
    public class TomlFronMatterBlockRoundtripeRenderer : RoundtripObjectRenderer<TomlFrontMatterBlock>
    {
        protected override void Write(RoundtripRenderer renderer, TomlFrontMatterBlock obj)
        {
            renderer.Write(obj.TriviaBefore);

            renderer.Write("+++\n");
#if true
            foreach (var line in obj.Lines.Lines)
            {
                if (line.Slice.Length > 0)
                {
                    renderer.Write(line);
                }
                if (line.NewLine != NewLine.None)
                {
                    renderer.Write(line.NewLine.AsString());
                }
            }
#else
            var tomlModel = obj.GetModel();
            var tomlString = Tomlyn.Toml.FromModel(tomlModel);
            renderer.Write(tomlString);
#endif
            renderer.Write("+++\n");

            renderer.Write(obj.TriviaAfter);
        }
    }
}
