using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GlogGenerator.Data;
using GlogGenerator.MarkdownExtensions;
using GlogGenerator.TemplateRenderers;
using Markdig;
using Markdig.Extensions.ListExtras;
using Markdig.Syntax;
using Microsoft.AspNetCore.StaticFiles;

namespace GlogGenerator.RenderState
{
    public class PageState : IOutputContent
    {
        public string HashCode
        {
            get
            {
                var pageHashInBytes = Encoding.UTF8.GetBytes(this.Permalink);
                var pageHashOutBytes = SHA256.HashData(pageHashInBytes);
                var pageHash = Convert.ToHexString(pageHashOutBytes);

                return pageHash;
            }
        }

        public List<CategoryData> Categories { get; set; } = new List<CategoryData>();

        public DateTimeOffset Date { get; set; } = DateTimeOffset.MinValue;

        public List<GameData> Games { get; set; } = new List<GameData>();

        public bool HideDate { get; set; }

        public bool HideTitle { get; set; }

        // This path should ALWAYS use unix-style path separators '/'
        public string OutputPathRelative { get; set; } = string.Empty;

        public string Permalink { get; set; } = string.Empty;

        public List<PlatformData> Platforms { get; set; } = new List<PlatformData>();

        public List<RatingData> Ratings { get; set; } = new List<RatingData>();

        public string RenderTemplateName { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new List<string>();

        public List<string> RelatedGames { get; set; } = new List<string>();

        public string Title { get; set; } = string.Empty;

        public string IgdbUrl { get; set; }

        public MarkdownDocument SourceContent { get; set; }

        public string RenderedContent
        {
            get
            {
                if (this.renderedContent == null)
                {
                    this.renderedContent = this.RenderContentFromSourceMarkdown();
                }

                return this.renderedContent;
            }
        }

        public string ContentEscapedForRss
        {
            get
            {
                return this.RenderedContent
                    .Trim()
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&#34;")
                    .Replace("'", "&#39;");
            }
        }

        public string PageType { get; set; } = string.Empty;

        public List<PageState> HistoryPosts { get; set; } = new List<PageState>();

        public bool HidePrevLink { get; set; } = true;

        public string PrevLinkRelative { get; set; } = string.Empty;

        public bool HideNextLink { get; set; } = true;

        public string NextLinkRelative { get; set; } = string.Empty;

        public int LinkedPostsCount
        {
            get
            {
                return this.LinkedPosts.Count;
            }
        }

        public List<LinkedPostProperties> LinkedPosts { get; set; } = new List<LinkedPostProperties>();

        public int TermsCount
        {
            get
            {
                return this.Terms.Count;
            }
        }

        public List<string> Terms { get; set; } = new List<string>();

        public string TermsType { get; set; } = string.Empty;

        private readonly SiteBuilder siteBuilder;

        private string renderedContent;

        public PageState(SiteBuilder siteBuilder)
        {
            this.siteBuilder = siteBuilder;
        }

        private string RenderContentFromSourceMarkdown()
        {
            if (this.SourceContent.Count == 0)
            {
                return string.Empty;
            }

            var rendererContext = this.siteBuilder.GetRendererContext();
            rendererContext.SetPageHashCode(this.HashCode);
            var rendered = this.siteBuilder.RenderHtml(this.SourceContent);
            rendererContext.Clear();

            return rendered;
        }

        public static PageState FromPageData(SiteBuilder siteBuilder, PageData pageData)
        {
            var page = new PageState(siteBuilder);
            page.HideDate = true;
            page.HideTitle = true;

            page.SourceContent = pageData.MdDoc;

            page.Permalink = $"{siteBuilder.GetBaseURL()}{pageData.PermalinkRelative}";

            var outputPathRelative = pageData.PermalinkRelative;
            if (!outputPathRelative.EndsWith('/'))
            {
                outputPathRelative += '/';
            }
            outputPathRelative += "index.html";

            page.OutputPathRelative = outputPathRelative;
            page.RenderTemplateName = "single";

            return page;
        }

        public static PageState FromPostData(SiteBuilder siteBuilder, PostData postData, ISiteDataIndex siteDataIndex)
        {
            var page = new PageState(siteBuilder);

            page.Categories = postData.Categories.Select(r => siteDataIndex.GetData(r)).ToList();

            page.Date = postData.Date;

            page.Games = postData.Games?.Select(r => siteDataIndex.GetData(r)).ToList();

            page.HideDate = (postData.Date == DateTimeOffset.MinValue);

            page.Permalink = $"{siteBuilder.GetBaseURL()}{postData.PermalinkRelative}";

            page.Platforms = postData.Platforms?.Select(r => siteDataIndex.GetData(r)).ToList();

            page.Ratings = postData.Ratings?.Select(r => siteDataIndex.GetData(r)).ToList();

            page.Title = postData.Title;

            page.SourceContent = postData.MdDoc;

            var outputPathRelative = postData.PermalinkRelative;
            if (!outputPathRelative.EndsWith('/'))
            {
                outputPathRelative += '/';
            }
            outputPathRelative += "index.html";

            page.OutputPathRelative = outputPathRelative;
            page.RenderTemplateName = "single";

            return page;
        }

        public static PageState FromCategoryData(SiteBuilder siteBuilder, CategoryData categoryData, ISiteDataIndex siteDataIndex)
        {
            var page = new PageState(siteBuilder);

            page.Permalink = $"{siteBuilder.GetBaseURL()}{categoryData.GetPermalinkRelative()}";

            page.Title = categoryData.Name;

            page.PageType = "categories";

            page.LinkedPosts = categoryData.LinkedPostIds.Select(i => siteDataIndex.GetPostById(i))
                .Select(p => LinkedPostProperties.FromPostData(p, siteDataIndex))
                .OrderByDescending(p => p.Date).ToList();

            if (page.LinkedPostsCount > 0)
            {
                page.Date = page.LinkedPosts.Select(p => p.Date).Max();
            }
            else
            {
                page.HideDate = true;
            }

            page.OutputPathRelative = $"{categoryData.GetPermalinkRelative()}index.html";
            page.RenderTemplateName = "list";

            return page;
        }

        public static PageState FromGameData(SiteBuilder siteBuilder, GameData gameData, ISiteDataIndex siteDataIndex)
        {
            var page = new PageState(siteBuilder);

            page.Permalink = $"{siteBuilder.GetBaseURL()}{gameData.GetPermalinkRelative()}";

            page.Title = gameData.Title;

            page.PageType = "games";

            page.IgdbUrl = gameData.IgdbUrl;
            page.Tags = gameData.Tags.ToList();
            page.RelatedGames = gameData.RelatedGames.OrderBy(s => s).ToList();

            page.LinkedPosts = gameData.LinkedPostIds.Select(i => siteDataIndex.GetPostById(i))
                .Select(p => LinkedPostProperties.FromPostData(p, siteDataIndex))
                .OrderByDescending(p => p.Date).ToList();

            if (page.LinkedPostsCount > 0)
            {
                page.Date = page.LinkedPosts.Select(p => p.Date).Max();
            }
            else
            {
                page.HideDate = true;
            }

            page.OutputPathRelative = $"{gameData.GetPermalinkRelative()}index.html";
            page.RenderTemplateName = "list_game";

            return page;
        }

        public static PageState FromPlatformData(SiteBuilder siteBuilder, PlatformData platformData, ISiteDataIndex siteDataIndex)
        {
            var page = new PageState(siteBuilder);

            page.Permalink = $"{siteBuilder.GetBaseURL()}{platformData.GetPermalinkRelative()}";

            if (!string.IsNullOrEmpty(platformData.Name))
            {
                if (platformData.Name.Contains(platformData.Abbreviation, StringComparison.Ordinal))
                {
                    page.Title = platformData.Name;
                }
                else
                {
                    page.Title = $"{platformData.Name} ({platformData.Abbreviation})";
                }
            }
            else
            {
                page.Title = platformData.Abbreviation;
            }

            page.PageType = "platforms";

            page.IgdbUrl = platformData.IgdbUrl;

            page.LinkedPosts = platformData.LinkedPostIds.Select(i => siteDataIndex.GetPostById(i))
                .Select(p => LinkedPostProperties.FromPostData(p, siteDataIndex))
                .OrderByDescending(p => p.Date).ToList();

            if (page.LinkedPostsCount > 0)
            {
                page.Date = page.LinkedPosts.Select(p => p.Date).Max();
            }
            else
            {
                page.HideDate = true;
            }

            page.OutputPathRelative = $"{platformData.GetPermalinkRelative()}index.html";
            page.RenderTemplateName = "list_platform";

            return page;
        }

        public static PageState FromRatingData(SiteBuilder siteBuilder, RatingData ratingData, ISiteDataIndex siteDataIndex)
        {
            var page = new PageState(siteBuilder);

            page.Permalink = $"{siteBuilder.GetBaseURL()}{ratingData.GetPermalinkRelative()}";

            page.Title = ratingData.Name;

            page.PageType = "ratings";

            page.LinkedPosts = ratingData.LinkedPostIds.Select(i => siteDataIndex.GetPostById(i))
                .Select(p => LinkedPostProperties.FromPostData(p, siteDataIndex))
                .OrderByDescending(p => p.Date).ToList();

            if (page.LinkedPostsCount > 0)
            {
                page.Date = page.LinkedPosts.Select(p => p.Date).Max();
            }
            else
            {
                page.HideDate = true;
            }

            page.OutputPathRelative = $"{ratingData.GetPermalinkRelative()}index.html";
            page.RenderTemplateName = "list";

            return page;
        }

        public static PageState FromTagData(SiteBuilder siteBuilder, TagData tagData, ISiteDataIndex siteDataIndex)
        {
            var page = new PageState(siteBuilder);

            page.Permalink = $"{siteBuilder.GetBaseURL()}{tagData.GetPermalinkRelative()}";

            page.Title = tagData.Name;

            page.PageType = "tags";

            page.LinkedPosts = tagData.LinkedPostIds.Select(i => siteDataIndex.GetPostById(i))
                .Select(p => LinkedPostProperties.FromPostData(p, siteDataIndex))
                .OrderByDescending(p => p.Date).ToList();

            if (page.LinkedPostsCount > 0)
            {
                page.Date = page.LinkedPosts.Select(p => p.Date).Max();
            }
            else
            {
                page.HideDate = true;
            }

            page.OutputPathRelative = $"{tagData.GetPermalinkRelative()}index.html";
            page.RenderTemplateName = "list_tag";

            return page;
        }

        public void WriteFile(SiteState site, string filePath)
        {
            var template = site.GetTemplateGroup().GetInstanceOf(this.RenderTemplateName);
            template.Add("site", site);
            template.Add("page", this);

            var rendered = template.RenderWithErrorsThrown(CultureInfo.InvariantCulture);
            rendered = rendered.ReplaceLineEndings("\n");

            var outputDir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(outputDir))
            {
                throw new ArgumentException($"Page output path {filePath} has empty dirname");
            }

            var canonicalOutputDirPath = outputDir + Path.DirectorySeparatorChar;
            if (!Directory.Exists(canonicalOutputDirPath))
            {
                Directory.CreateDirectory(canonicalOutputDirPath);
            }

            var outputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            File.WriteAllText(filePath, rendered, outputEncoding);
        }

        public void WriteHttpListenerResponse(SiteState site, ref HttpListenerResponse response)
        {
            var typeProvider = new FileExtensionContentTypeProvider();
            if (typeProvider.TryGetContentType(this.OutputPathRelative, out var contentType))
            {
                response.ContentType = contentType;
            }
            else
            {
                response.ContentType = "text/html";
            }

            var template = site.GetTemplateGroup().GetInstanceOf(this.RenderTemplateName);
            template.Add("site", site);
            template.Add("page", this);

            var rendered = template.RenderWithErrorsThrown(CultureInfo.InvariantCulture);
            var renderedBytes = Encoding.UTF8.GetBytes(rendered);

            response.ContentLength64 = renderedBytes.LongLength;

            using (var byteWriter = new BinaryWriter(response.OutputStream))
            {
                byteWriter.Write(renderedBytes, 0, renderedBytes.Length);

                byteWriter.Close();
            }
        }
    }
}
