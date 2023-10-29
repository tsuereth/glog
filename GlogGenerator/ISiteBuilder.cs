using Markdig;

namespace GlogGenerator
{
    public interface ISiteBuilder
    {
        public MarkdownPipeline GetMarkdownPipeline();

        public IIgdbCache GetIgdbCache();
    }
}
