using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.Data;
using GlogGenerator.TemplateRenderers;
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

        private Antlr4.StringTemplate.TemplateGroupDirectory templateGroup;

        public SiteState(
            SiteBuilder builder,
            string templateFilesBasePath)
        {
            this.builder = builder;
            this.templateFilesBasePath = templateFilesBasePath;
        }

        public Antlr4.StringTemplate.TemplateGroupDirectory GetTemplateGroup()
        {
            if (this.templateGroup == null)
            {
                // StringTemplate requires an absolute filepath.
                var templateFilesBasePath = this.templateFilesBasePath;
                if (!Path.IsPathRooted(templateFilesBasePath))
                {
                    templateFilesBasePath = Path.GetFullPath(templateFilesBasePath);
                }

                this.templateGroup = new Antlr4.StringTemplate.TemplateGroupDirectory(
                    templateFilesBasePath,
                    delimiterStartChar: '%',
                    delimiterStopChar: '%');

                this.templateGroup.RegisterRenderer(typeof(DateTimeOffset), new DateTimeRenderer());
                this.templateGroup.RegisterRenderer(typeof(string), new StringRenderer());
            }

            return this.templateGroup;
        }

        public void LoadContentRoutes()
        {
            this.ContentRoutes = this.builder.ResolveContentRoutes();
        }
    }
}
