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

        private ScribanTemplateLoader templateLoader;

        public SiteState(
            SiteBuilder builder,
            string templateFilesBasePath)
        {
            this.builder = builder;
            this.templateFilesBasePath = templateFilesBasePath;
        }

        public ScribanTemplateLoader GetTemplateLoader()
        {
            if (this.templateLoader == null)
            {
                // Make sure the path to template files is an absolute filepath.
                var templateFilesBasePath = this.templateFilesBasePath;
                if (!Path.IsPathRooted(templateFilesBasePath))
                {
                    templateFilesBasePath = Path.GetFullPath(templateFilesBasePath);
                }

                this.templateLoader = new ScribanTemplateLoader(templateFilesBasePath);
            }

            return this.templateLoader;
        }

        public void LoadContentRoutes()
        {
            this.ContentRoutes = this.builder.ResolveContentRoutes();
        }
    }
}
