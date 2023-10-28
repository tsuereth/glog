using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.Data;
using GlogGenerator.MarkdownExtensions;
using GlogGenerator.TemplateRenderers;
using Markdig;
using Markdig.Extensions.ListExtras;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;

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

        public DateTimeOffset BuildDate { get; set; } = DateTimeOffset.MinValue;

        public List<string> CategoriesSorted
        {
            get
            {
                return this.dataIndex.GetCategories().Select(c => c.Name).OrderBy(c => c).ToList();
            }
        }

        public List<string> NowPlaying
        {
            get
            {
                return this.builder.GetNowPlaying();
            }
        }

        public Dictionary<string, IOutputContent> ContentRoutes { get; set; } = new Dictionary<string, IOutputContent>();

        private readonly ILogger logger;
        private readonly SiteBuilder builder;
        private readonly SiteDataIndex dataIndex;
        private readonly string templateFilesBasePath;

        private GlogMarkdownExtension glogMarkdownExtension;
        private MarkdownPipeline markdownPipeline;
        private Antlr4.StringTemplate.TemplateGroupDirectory templateGroup;

        public SiteState(
            ILogger logger,
            SiteBuilder builder,
            SiteDataIndex dataIndex,
            string templateFilesBasePath)
        {
            this.logger = logger;
            this.builder = builder;
            this.dataIndex = dataIndex;
            this.templateFilesBasePath = templateFilesBasePath;

            this.BuildDate = DateTimeOffset.Now;

            this.glogMarkdownExtension = new GlogMarkdownExtension(this.builder, this.dataIndex, this);
            this.markdownPipeline = new MarkdownPipelineBuilder()
                .Use<ListExtraExtension>()
                .UseGenericAttributes()
                .UseMediaLinks()
                .UsePipeTables()
                .UseSoftlineBreakAsHardlineBreak()
                .Use(this.glogMarkdownExtension)
                .Build();
        }

        public MarkdownDocument ParseMarkdown(string markdown)
        {
            return Markdown.Parse(markdown, this.markdownPipeline);
        }

        public string RenderHtml(MarkdownDocument parsedDocument, string pageHashCode)
        {
            var rendererContext = this.glogMarkdownExtension.GetRendererContext();
            rendererContext.SetPageHashCode(pageHashCode);

            var html = parsedDocument.ToHtml(this.markdownPipeline);

            rendererContext.Clear();

            return html;
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

        public void LoadSiteRoutes()
        {
            this.ContentRoutes.Clear();

            var staticFiles = this.dataIndex.GetStaticFiles();
            foreach (var staticFile in staticFiles)
            {
                try
                {
                    var outputFile = StaticFileState.FromStaticFileData(this, staticFile);
                    this.ContentRoutes.Add(outputFile.OutputPathRelative, outputFile);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to generate static content from file {staticFile.SourceFilePath}", ex);
                }
            }

            var posts = this.dataIndex.GetPosts();
            var postPages = new List<PageState>(posts.Count);
            foreach (var postData in posts)
            {
                try
                {
                    // Verify that the post's games are found in our metadata cache.
                    foreach (var game in postData.Games)
                    {
                        _ = this.dataIndex.ValidateMatchingGameName(game);
                    }

                    var page = PageState.FromPostData(this.builder, this, postData);
                    postPages.Add(page);
                    this.ContentRoutes.Add(page.OutputPathRelative, page);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to generate page from post {postData.SourceFilePath}", ex);
                }
            }

            var postsListPage = new PageState(this.builder, this)
            {
                HideDate = true,
                Title = "Posts",
                PageType = "posts",
                Permalink = $"{this.BaseURL}post/",
                OutputPathRelative = "post/index.html",
                RenderTemplateName = "list",
                LinkedPosts = posts,
            };
            this.ContentRoutes.Add(postsListPage.OutputPathRelative, postsListPage);

            const int pagesPerHistoryPage = 10;
            for (var historyPageNum = 0; (historyPageNum * pagesPerHistoryPage) < postPages.Count; ++historyPageNum)
            {
                var historyPage = new PageState(this.builder, this)
                {
                    HideDate = true,
                    HideTitle = true,
                    Permalink = this.BaseURL, // BUG?: every history page has the BaseURL permalink!
                    RenderTemplateName = "history",
                };

                var pageOneBased = historyPageNum + 1;

                var firstPostNum = historyPageNum * pagesPerHistoryPage;
                var postsCount = pagesPerHistoryPage;
                if (firstPostNum + postsCount >= postPages.Count)
                {
                    postsCount = postPages.Count - firstPostNum;
                }

                historyPage.HistoryPosts = postPages.GetRange(firstPostNum, postsCount);

                if (historyPageNum == 0)
                {
                    historyPage.HidePrevLink = true;
                }
                else if (historyPageNum == 1)
                {
                    historyPage.HidePrevLink = false;
                    historyPage.PrevLinkRelative = string.Empty; // special case! link to BaseURL.
                }
                else
                {
                    historyPage.HidePrevLink = false;
                    var prevPageOneBased = pageOneBased - 1;
                    historyPage.PrevLinkRelative = $"page/{prevPageOneBased}/";
                }

                if (((historyPageNum + 1) * pagesPerHistoryPage) >= postPages.Count)
                {
                    historyPage.HideNextLink = true;
                }
                else
                {
                    historyPage.HideNextLink = false;
                    var nextPageOneBased = pageOneBased + 1;
                    historyPage.NextLinkRelative = $"page/{nextPageOneBased}/";
                }

                if (historyPageNum == 0)
                {
                    historyPage.OutputPathRelative = "index.html";
                }
                else
                {
                    historyPage.OutputPathRelative = $"page/{pageOneBased}/index.html";
                }

                this.ContentRoutes.Add(historyPage.OutputPathRelative, historyPage);
            }

            var rssFeedItems = Math.Min(postPages.Count, 15);
            var rssFeedPage = new PageState(this.builder, this)
            {
                Date = posts[0].Date,
                OutputPathRelative = "index.xml",
                Permalink = this.BaseURL,
                RenderTemplateName = "rss",
                HistoryPosts = postPages.GetRange(0, rssFeedItems),
            };
            this.ContentRoutes.Add(rssFeedPage.OutputPathRelative, rssFeedPage);

            var categoriesIndex = new PageState(this.builder, this)
            {
                HideDate = true,
                OutputPathRelative = "category/index.html",
                Permalink = $"{this.BaseURL}category/",
                RenderTemplateName = "termslist",
                Title = "Categories",
                PageType = "categories",
                Terms = this.CategoriesSorted,
                TermsType = "category",
            };
            this.ContentRoutes.Add(categoriesIndex.OutputPathRelative, categoriesIndex);

            var categories = this.dataIndex.GetCategories();
            foreach (var categoryData in categories)
            {
                var page = PageState.FromCategoryData(this.builder, this, categoryData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var games = this.dataIndex.GetGames();
            var gamesIndex = new PageState(this.builder, this)
            {
                HideDate = true,
                OutputPathRelative = "game/index.html",
                Permalink = $"{this.BaseURL}game/",
                RenderTemplateName = "termslist",
                Title = "Games",
                PageType = "games",
                Terms = games.Select(g => g.Title).OrderBy(t => t, StringComparer.Ordinal).ToList(),
                TermsType = "game",
            };
            this.ContentRoutes.Add(gamesIndex.OutputPathRelative, gamesIndex);

            foreach (var gameData in games)
            {
                var page = PageState.FromGameData(this.builder, this, gameData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var platforms = this.dataIndex.GetPlatforms();
            var platformsIndex = new PageState(this.builder, this)
            {
                HideDate = true,
                OutputPathRelative = "platform/index.html",
                Permalink = $"{this.BaseURL}platform/",
                RenderTemplateName = "termslist",
                Title = "Platforms",
                PageType = "platforms",
                Terms = platforms.Select(p => p.Abbreviation).OrderBy(n => n, StringComparer.Ordinal).ToList(),
                TermsType = "platform",
            };
            this.ContentRoutes.Add(platformsIndex.OutputPathRelative, platformsIndex);

            foreach (var platformData in platforms)
            {
                var page = PageState.FromPlatformData(this.builder, this, platformData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var ratings = this.dataIndex.GetRatings();
            var ratingsIndex = new PageState(this.builder, this)
            {
                HideDate = true,
                OutputPathRelative = "rating/index.html",
                Permalink = $"{this.BaseURL}rating/",
                RenderTemplateName = "termslist",
                Title = "Ratings",
                PageType = "ratings",
                Terms = ratings.Select(r => r.Name).OrderBy(n => n).ToList(),
                TermsType = "rating",
            };
            this.ContentRoutes.Add(ratingsIndex.OutputPathRelative, ratingsIndex);

            foreach (var ratingData in ratings)
            {
                var page = PageState.FromRatingData(this.builder, this, ratingData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var tags = this.dataIndex.GetTags();
            var tagsIndex = new PageState(this.builder, this)
            {
                HideDate = true,
                OutputPathRelative = "tag/index.html",
                Permalink = $"{this.BaseURL}tag/",
                RenderTemplateName = "termslist",
                Title = "Tags",
                PageType = "tags",
                Terms = tags.Select(t => t.Name).OrderBy(n => n, StringComparer.Ordinal).ToList(),
                TermsType = "tag",
            };
            this.ContentRoutes.Add(tagsIndex.OutputPathRelative, tagsIndex);

            foreach (var tagData in tags)
            {
                var page = PageState.FromTagData(this.builder, this, tagData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var pages = this.dataIndex.GetPages();
            foreach (var pageData in pages)
            {
                var page = PageState.FromPageData(this.builder, this, pageData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }
        }
    }
}
