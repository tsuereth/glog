using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using GlogGenerator.Data;
using GlogGenerator.HugoCompat;
using Markdig;
using Markdig.Extensions.ListExtras;
using Microsoft.AspNetCore.StaticFiles;

namespace GlogGenerator.RenderState
{
    public class PageState : IOutputContent
    {
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

        public string Title { get; set; } = string.Empty;

        public string IgdbUrl { get; set; }

        public string SourceContent { get; set; } = string.Empty;

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

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        public int TermsCount
        {
            get
            {
                return this.Terms.Count;
            }
        }

        public List<string> Terms { get; set; } = new List<string>();

        public string TermsType { get; set; } = string.Empty;

        private SiteState siteState;
        private string renderedContent;

        public PageState(SiteState siteState)
        {
            this.siteState = siteState;
        }

        private string RenderContentFromSourceMarkdown()
        {
            if (string.IsNullOrEmpty(this.SourceContent))
            {
                return string.Empty;
            }

            var rendered = this.SourceContent;

            rendered = Regex.Replace(rendered, @"<ul>  +", "<ul>");
            rendered = Regex.Replace(rendered, @">\n<", ">__mdquirk_linebreak<", RegexOptions.Singleline);
            rendered = Regex.Replace(rendered, @"(\S)\n<ul>", "$1__mdquirk_linebreak<ul>", RegexOptions.Singleline);
            rendered = Regex.Replace(rendered, @"  +\n", "<br />");
            rendered = Regex.Replace(rendered, @"(<li>.+?)(?!</li><br />)", "$1"); // OMG FIXME PLZ
            rendered = Regex.Replace(rendered, @"(<li>.+?)(</li><br />)", "$1$2\n"); // OMG FIXME PLZ
            rendered = Regex.Replace(rendered, @"(\d+)\) ", "$1__mdquirk_numbereditemparen ");

            rendered = Shortcodes.TranslateToHtml(this.siteState.PathResolver, this.siteState, this, rendered);

            var mdPipeline = new MarkdownPipelineBuilder()
                .Use<ListExtraExtension>()
                .Use<MarkdownQuirksMarkdigExtension>()
                .Build();
            rendered = "\t" + Markdown.ToHtml(rendered, mdPipeline);

            rendered = rendered.Replace("__mdquirk_linebreak", "\n");

            rendered = rendered.Replace("__mdquirk_numbereditemparen", ")");

            rendered = Regex.Replace(rendered, @"<p><p(.+?)</p></p>", "<p$1</p>");

            rendered = rendered.Replace("</blockquote>", "</blockquote>\n");
            rendered = rendered.Replace("<br />", "<br />\n");
            rendered = rendered.Replace("</div>", "</div>\n");
            rendered = rendered.Replace("</ol>", "</ol>\n");
            rendered = rendered.Replace("</table>", "</table>\n");
            rendered = rendered.Replace("</ul>\n<p>", "</ul>\n\n<p>");

            rendered = rendered.Replace("</p>", "</p>\n");
            rendered = rendered.Replace("<script", "\n<script");
            rendered = rendered.Replace("</p>\n\n</li>", "</p></li>\n");

            rendered = rendered.Replace("</li>\n\n</ul>", "</li>\n</ul>");
            rendered = rendered.Replace("</noscript>\n\n<div", "</noscript>\n<div");
            rendered = rendered.Replace("</ol>\n<br />", "</ol><br />");

            // BUG: Markdown.ToHtml is escaping the '&' part of HTML escape sequences.
            // MarkdownQuirksMarkdigExtension should be disabling this behavior, but...
            // *some* HTML escaping is needed to match Hugo's/Blackfriday's &quot; proliferation.
            rendered = Regex.Replace(rendered, @"&amp;(\w+);", "&$1;");

            rendered = rendered.Replace("é", "&eacute;");
            rendered = rendered.Replace("∀", "&forall;");
            rendered = rendered.Replace("¡", "&iexcl;");
            rendered = rendered.Replace("ó", "&oacute;");
            rendered = rendered.Replace("ú", "&uacute;");
            rendered = rendered.Replace("ü", "&uuml;");
            rendered = rendered.Replace("👍", "&#x1F44D;");

            rendered = rendered.Replace(
                "<p><noscript><i>A Google Chart would go here, but JavaScript is disabled.</i></noscript></p>\n",
                "<noscript><i>A Google Chart would go here, but JavaScript is disabled.</i></noscript>");

            return rendered;
        }

        public static PageState FromPageData(SiteState site, PageData pageData)
        {
            var page = new PageState(site);
            page.HideDate = true;
            page.HideTitle = true;

            page.SourceContent = pageData.Content;

            page.Permalink = $"{site.BaseURL}{pageData.PermalinkRelative}";

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

        public static PageState FromPostData(SiteState site, PostData postData)
        {
            // Verify that the post's games are found in our metadata cache.
            foreach (var game in postData.Games)
            {
                _ = site.ValidateMatchingGameName(game);
            }

            var page = new PageState(site);

            page.Categories = postData.Categories.Select(c => new CategoryData() { Name = c }).ToList();

            page.Date = postData.Date;

            page.Games = postData.Games.Select(c => new GameData() { Title = c }).ToList();

            page.HideDate = (postData.Date == DateTimeOffset.MinValue);

            page.Permalink = $"{site.BaseURL}{postData.PermalinkRelative}";

            page.Platforms = postData.Platforms.Select(c => new PlatformData() { Name = c }).ToList();

            page.Ratings = postData.Ratings.Select(c => new RatingData() { Name = c }).ToList();

            page.Title = postData.Title;

            page.SourceContent = postData.Content;

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

        public static PageState FromCategoryData(SiteState site, CategoryData categoryData)
        {
            var page = new PageState(site);

            page.Permalink = $"{site.BaseURL}{categoryData.PermalinkRelative}";

            page.Title = categoryData.Name;

            page.PageType = "categories";

            page.LinkedPosts = categoryData.LinkedPosts.OrderByDescending(p => p.Date).ToList();

            if (page.LinkedPostsCount > 0)
            {
                page.Date = page.LinkedPosts.Select(p => p.Date).Max();
            }
            else
            {
                page.HideDate = true;
            }

            page.OutputPathRelative = $"{categoryData.PermalinkRelative}index.html";
            page.RenderTemplateName = "list";

            return page;
        }

        public static PageState FromGameData(SiteState site, GameData gameData)
        {
            var page = new PageState(site);

            page.Permalink = $"{site.BaseURL}{gameData.PermalinkRelative}";

            page.Title = gameData.Title;

            page.PageType = "games";

            page.IgdbUrl = gameData.IgdbUrl;
            page.Tags = gameData.Tags;

            page.LinkedPosts = gameData.LinkedPosts.OrderByDescending(p => p.Date).ToList();

            if (page.LinkedPostsCount > 0)
            {
                page.Date = page.LinkedPosts.Select(p => p.Date).Max();
            }
            else
            {
                page.HideDate = true;
            }

            page.OutputPathRelative = $"{gameData.PermalinkRelative}index.html";
            page.RenderTemplateName = "list_game";

            return page;
        }

        public static PageState FromPlatformData(SiteState site, PlatformData platformData)
        {
            var page = new PageState(site);

            page.Permalink = $"{site.BaseURL}{platformData.PermalinkRelative}";

            page.Title = platformData.Name;

            page.PageType = "platforms";

            page.LinkedPosts = platformData.LinkedPosts.OrderByDescending(p => p.Date).ToList();

            if (page.LinkedPostsCount > 0)
            {
                page.Date = page.LinkedPosts.Select(p => p.Date).Max();
            }
            else
            {
                page.HideDate = true;
            }

            page.OutputPathRelative = $"{platformData.PermalinkRelative}index.html";
            page.RenderTemplateName = "list";

            return page;
        }

        public static PageState FromRatingData(SiteState site, RatingData ratingData)
        {
            var page = new PageState(site);

            page.Permalink = $"{site.BaseURL}{ratingData.PermalinkRelative}";

            page.Title = ratingData.Name;

            page.PageType = "ratings";

            page.LinkedPosts = ratingData.LinkedPosts.OrderByDescending(p => p.Date).ToList();

            if (page.LinkedPostsCount > 0)
            {
                page.Date = page.LinkedPosts.Select(p => p.Date).Max();
            }
            else
            {
                page.HideDate = true;
            }

            page.OutputPathRelative = $"{ratingData.PermalinkRelative}index.html";
            page.RenderTemplateName = "list";

            return page;
        }

        public static PageState FromTagData(SiteState site, TagData tagData)
        {
            var page = new PageState(site);

            page.Permalink = $"{site.BaseURL}{tagData.PermalinkRelative}";

            page.Title = tagData.Name;

            page.PageType = "tags";

            page.LinkedPosts = tagData.LinkedPosts.OrderByDescending(p => p.Date).ToList();

            if (page.LinkedPostsCount > 0)
            {
                page.Date = page.LinkedPosts.Select(p => p.Date).Max();
            }
            else
            {
                page.HideDate = true;
            }

            page.OutputPathRelative = $"{tagData.PermalinkRelative}index.html";
            page.RenderTemplateName = "list_tag";

            return page;
        }

        public void WriteFile(SiteState site, string filePath)
        {
            var template = site.GetTemplateGroup().GetInstanceOf(this.RenderTemplateName);
            template.Add("site", site);
            template.Add("page", this);

            var rendered = template.Render(CultureInfo.InvariantCulture);
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

            var rendered = template.Render(CultureInfo.InvariantCulture);
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
