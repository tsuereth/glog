using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.Data;
using Markdig.Syntax;

namespace GlogGenerator.RenderState
{
    public class SiteState
    {
        public string BaseURL
        {
            get
            {
                return this.builder.GetBaseURL();
            }
        }

        public DateTimeOffset BuildDate
        {
            get
            {
                return this.builder.GetBuildDate();
            }
        }

        public List<string> CategoriesSorted
        {
            get
            {
                return this.builder.GetCategories();
            }
        }

        public List<string> NowPlaying
        {
            get
            {
                return this.builder.GetNowPlaying();
            }
        }

        public Dictionary<string, IOutputContent> ContentRoutes { get; private set; } = new Dictionary<string, IOutputContent>();

        private readonly SiteBuilder builder;
        private readonly string templateFilesBasePath;

        public SiteState(
            SiteBuilder builder,
            string templateFilesBasePath)
        {
            this.builder = builder;
            this.templateFilesBasePath = templateFilesBasePath;
        }

        public ScribanTemplateLoader CreateTemplateLoader()
        {
            // StringTemplate requires an absolute filepath.
            // BUT DOES SCRIBAN ?!?!?!?!?!?!
            var templateFilesBasePath = this.templateFilesBasePath;
            if (!Path.IsPathRooted(templateFilesBasePath))
            {
                templateFilesBasePath = Path.GetFullPath(templateFilesBasePath);
            }

            return new ScribanTemplateLoader(templateFilesBasePath);

            /*this.templateGroup = new Antlr4.StringTemplate.TemplateGroupDirectory(
                templateFilesBasePath,
                delimiterStartChar: '%',
                delimiterStopChar: '%');

            this.templateGroup.RegisterRenderer(typeof(DateTimeOffset), new DateTimeRenderer());
            this.templateGroup.RegisterRenderer(typeof(string), new StringRenderer());*/
        }

        public void LoadContentRoutes()
        {
            this.ContentRoutes = this.builder.ResolveContentRoutes();
        }
    }
}
