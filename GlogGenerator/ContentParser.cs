using GlogGenerator.Data;
using GlogGenerator.MarkdownExtensions;
using GlogGenerator.RenderState;
using Markdig;

namespace GlogGenerator
{
    public class ContentParser
    {
        private VariableSubstitution variableSubstitution;

        private GlogMarkdownExtension glogMarkdownExtension;

        // Separate markdown (Markdig) pipelines are needed for HTML rendering versus source upkeep (roundtrips).
        // Some normalization/roundtrip settings are detrimental to HTML output!
        // Particularly, TrackTrivia captures spacing that _isn't intended_ to render in HTML.
        // https://github.com/xoofx/markdig/issues/561#issuecomment-1064848909
        private MarkdownPipeline markdownToHtmlPipeline;
        private MarkdownPipeline markdownRoundtripPipeline;

        public ContentParser(VariableSubstitution rendererVariableSubstitution, ISiteDataIndex siteDataIndex, SiteState siteState)
        {
            this.variableSubstitution = rendererVariableSubstitution;
            this.glogMarkdownExtension = new GlogMarkdownExtension(rendererVariableSubstitution, siteDataIndex, siteState);

            this.markdownToHtmlPipeline = new MarkdownPipelineBuilder()
                .UseEmphasisExtras()
                .UseMediaLinks()
                .UsePipeTables()
                .UseSoftlineBreakAsHardlineBreak()
                .Use(this.glogMarkdownExtension)
                .Build();

            this.markdownRoundtripPipeline = new MarkdownPipelineBuilder()
                .EnableTrackTrivia()
                .UseEmphasisExtras()
                .UseMediaLinks()
                .UsePipeTables()
                .UseSoftlineBreakAsHardlineBreak()
                .Use(this.glogMarkdownExtension)
                .Build();
        }

        public VariableSubstitution GetVariableSubstitution()
        {
            return variableSubstitution;
        }

        public GlogMarkdownExtension GetGlogMarkdownExtension()
        {
            return this.glogMarkdownExtension;
        }

        public MarkdownPipeline GetHtmlRenderPipeline()
        {
            return markdownToHtmlPipeline;
        }

        public MarkdownPipeline GetRoundtripRenderPipeline()
        {
            return markdownRoundtripPipeline;
        }
    }
}
