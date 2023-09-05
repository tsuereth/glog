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

        public string Content { get; set; } = string.Empty;

        public string ContentEscapedForRss
        {
            get
            {
                return this.Content
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

        private void TransformContentFromText(string text)
        {
            this.Content = text;

            this.Content = Regex.Replace(this.Content, @"<ul>  +", "<ul>");
            this.Content = Regex.Replace(this.Content, @">\n<", ">__mdquirk_linebreak<", RegexOptions.Singleline);
            this.Content = Regex.Replace(this.Content, @"(\S)\n<ul>", "$1__mdquirk_linebreak<ul>", RegexOptions.Singleline);
            this.Content = Regex.Replace(this.Content, @"  +\n", "<br />");
            this.Content = Regex.Replace(this.Content, @"(<li>.+?)(?!</li><br />)", "$1"); // OMG FIXME PLZ
            this.Content = Regex.Replace(this.Content, @"(<li>.+?)(</li><br />)", "$1$2\n"); // OMG FIXME PLZ
            this.Content = Regex.Replace(this.Content, @"(\d+)\) ", "$1__mdquirk_numbereditemparen ");
        }

        private void TransformMarkdownContent(SiteState site)
        {
            var content = Shortcodes.TranslateToHtml(site.PathResolver, site, this, this.Content);

            var mdPipeline = new MarkdownPipelineBuilder().Use<MarkdownQuirksMarkdigExtension>().Build();
            this.Content = "\t" + Markdown.ToHtml(content, mdPipeline);

            this.Content = this.Content.Replace("__mdquirk_linebreak", "\n");

            this.Content = this.Content.Replace("__mdquirk_numbereditemparen", ")");

            this.Content = Regex.Replace(this.Content, @"<p><p(.+?)</p></p>", "<p$1</p>");

            this.Content = this.Content.Replace("</blockquote>", "</blockquote>\n");
            this.Content = this.Content.Replace("<br />", "<br />\n");
            this.Content = this.Content.Replace("</div>", "</div>\n");
            this.Content = this.Content.Replace("</ol>", "</ol>\n");
            this.Content = this.Content.Replace("</table>", "</table>\n");
            this.Content = this.Content.Replace("</ul>\n<p>", "</ul>\n\n<p>");

            this.Content = this.Content.Replace("</p>", "</p>\n");
            this.Content = this.Content.Replace("<script", "\n<script");
            this.Content = this.Content.Replace("</p>\n\n</li>", "</p></li>\n");

            this.Content = this.Content.Replace("</li>\n\n</ul>", "</li>\n</ul>");
            this.Content = this.Content.Replace("</noscript>\n\n<div", "</noscript>\n<div");
            this.Content = this.Content.Replace("</ol>\n<br />", "</ol><br />");

            // BUG: Markdown.ToHtml is escaping the '&' part of HTML escape sequences.
            // MarkdownQuirksMarkdigExtension should be disabling this behavior, but...
            // *some* HTML escaping is needed to match Hugo's/Blackfriday's &quot; proliferation.
            this.Content = Regex.Replace(this.Content, @"&amp;(\w+);", "&$1;");

            this.Content = this.Content.Replace("é", "&eacute;");
            this.Content = this.Content.Replace("∀", "&forall;");
            this.Content = this.Content.Replace("¡", "&iexcl;");
            this.Content = this.Content.Replace("ó", "&oacute;");
            this.Content = this.Content.Replace("ú", "&uacute;");
            this.Content = this.Content.Replace("ü", "&uuml;");
            this.Content = this.Content.Replace("👍", "&#x1F44D;");

            this.Content = this.Content.Replace(
                "<p><noscript><i>A Google Chart would go here, but JavaScript is disabled.</i></noscript></p>\n",
                "<noscript><i>A Google Chart would go here, but JavaScript is disabled.</i></noscript>");
        }

        public static PageState FromPageData(SiteState site, PageData pageData)
        {
            var page = new PageState();
            page.HideDate = true;
            page.HideTitle = true;

            page.TransformContentFromText(pageData.Content);

            page.Permalink = $"{site.BaseURL}{pageData.PermalinkRelative}";

            page.TransformMarkdownContent(site);

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
                var cacheEntries = site.IgdbCache.GetGameByName(game);
                if (cacheEntries.Count == 0)
                {
                    throw new InvalidDataException($"No games in cache match the name \"{game}\"");
                }
                else if (cacheEntries.Count > 1)
                {
                    throw new InvalidDataException($"More than one game in cache matches the name \"{game}\"");
                }
            }

            var page = new PageState();

            page.Categories = postData.Categories.Select(c => new CategoryData() { Name = c }).ToList();

            page.TransformContentFromText(postData.Content);

            page.Date = postData.Date;

            page.Games = postData.Games.Select(c => new GameData() { Title = c }).ToList();

            page.HideDate = (postData.Date == DateTimeOffset.MinValue);

            page.Permalink = $"{site.BaseURL}{postData.PermalinkRelative}";

            page.Platforms = postData.Platforms.Select(c => new PlatformData() { Name = c }).ToList();

            page.Ratings = postData.Ratings.Select(c => new RatingData() { Name = c }).ToList();

            page.Title = postData.Title;

            page.TransformMarkdownContent(site);

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
            var page = new PageState();

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

            page.OutputPathRelative = $"{categoryData.OutputDirRelative}/index.html";
            page.RenderTemplateName = "list";

            return page;
        }

        public static PageState FromGameData(SiteState site, GameData gameData)
        {
            var page = new PageState();

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

            page.OutputPathRelative = $"{gameData.OutputDirRelative}/index.html";
            page.RenderTemplateName = "list_game";

            return page;
        }

        public static PageState FromPlatformData(SiteState site, PlatformData platformData)
        {
            var page = new PageState();

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

            page.OutputPathRelative = $"{platformData.OutputDirRelative}/index.html";
            page.RenderTemplateName = "list";

            return page;
        }

        public static PageState FromRatingData(SiteState site, RatingData ratingData)
        {
            var page = new PageState();

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

            page.OutputPathRelative = $"{ratingData.OutputDirRelative}/index.html";
            page.RenderTemplateName = "list";

            return page;
        }

        public static PageState FromTagData(SiteState site, TagData tagData)
        {
            var page = new PageState();

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

            page.OutputPathRelative = $"{tagData.OutputDirRelative}/index.html";
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
            response.ContentType = "text/html";

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
