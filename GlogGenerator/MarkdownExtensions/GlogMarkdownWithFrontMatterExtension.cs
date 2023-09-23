using Markdig;
using Markdig.Renderers;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogMarkdownWithFrontMatterExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.BlockParsers.AddIfNotAlready<TomlFrontMatterBlockParser>();
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
        }
    }
}
