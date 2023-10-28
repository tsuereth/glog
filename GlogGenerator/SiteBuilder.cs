using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.Data;
using GlogGenerator.MarkdownExtensions;
using GlogGenerator.RenderState;
using Markdig;
using Markdig.Extensions.ListExtras;
using Markdig.Syntax;

namespace GlogGenerator
{
    public class SiteBuilder : ISiteBuilder
    {
        private const string VariableNameSiteBaseURL = "SiteBaseURL";

        private readonly ConfigData configData;

        private DateTimeOffset buildDate;
        private VariableSubstitution variableSubstitution;
        private SiteDataIndex siteDataIndex;
        private SiteState siteState;
        private GlogMarkdownExtension glogMarkdownExtension;
        private MarkdownPipeline markdownPipeline;

        private IgdbCache igdbCache = null;

        public SiteBuilder() : this(new ConfigData()) { }

        public SiteBuilder(ConfigData configData)
        {
            this.configData = configData;

            this.buildDate = DateTimeOffset.Now;

            this.variableSubstitution = new VariableSubstitution();
            this.variableSubstitution.SetSubstitution(VariableNameSiteBaseURL, this.configData.BaseURL);

            this.siteDataIndex = new SiteDataIndex(this, this.configData.InputFilesBasePath);

            this.siteState = new SiteState(this, this.configData.TemplateFilesBasePath);

            this.glogMarkdownExtension = new GlogMarkdownExtension(this, this.siteDataIndex, this.siteState);

            this.markdownPipeline = new MarkdownPipelineBuilder()
                .Use<ListExtraExtension>()
                .UseGenericAttributes()
                .UseMediaLinks()
                .UsePipeTables()
                .UseSoftlineBreakAsHardlineBreak()
                .Use(this.glogMarkdownExtension)
                .Build();
        }

        public DateTimeOffset GetBuildDate()
        {
            return this.buildDate;
        }

        public void SetBuildDate(DateTimeOffset buildDate)
        {
            this.buildDate = buildDate;
        }

        public IIgdbCache GetIgdbCache()
        {
            if (this.igdbCache == null)
            {
                this.igdbCache = IgdbCache.FromJsonFile(this.configData.InputFilesBasePath);
            }

            return this.igdbCache;
        }

        public SiteDataIndex GetSiteDataIndex()
        {
            return this.siteDataIndex;
        }

        public SiteState GetSiteState()
        {
            return this.siteState;
        }

        public string GetBaseURL()
        {
            if (!this.variableSubstitution.TryGetSubstitution(VariableNameSiteBaseURL, out var baseURL))
            {
                // Since this variable is set in the constructor, it should never, ever be missing.
                throw new InvalidDataException();
            }

            return baseURL;
        }

        public void SetBaseURL(string baseURL)
        {
            this.variableSubstitution.SetSubstitution(VariableNameSiteBaseURL, baseURL);
        }

        public List<string> GetCategories()
        {
            return this.siteDataIndex.GetCategories().Select(c => c.Name).OrderBy(c => c).ToList();
        }

        public List<string> GetNowPlaying()
        {
            return this.configData.NowPlaying;
        }

        public VariableSubstitution GetVariableSubstitution()
        {
            return this.variableSubstitution;
        }

        public MarkdownDocument ParseMarkdown(string markdown)
        {
            return Markdown.Parse(markdown, this.markdownPipeline);
        }

        public HtmlRendererContext GetRendererContext()
        {
            return this.glogMarkdownExtension.GetRendererContext();
        }

        public string RenderHtml(MarkdownDocument parsedDocument)
        {
            return parsedDocument.ToHtml(this.markdownPipeline);
        }

        public Dictionary<string, IOutputContent> ResolveContentRoutes()
        {
            var contentRoutes = new Dictionary<string, IOutputContent>();

            var staticFiles = this.siteDataIndex.GetStaticFiles();
            foreach (var staticFile in staticFiles)
            {
                try
                {
                    var outputFile = StaticFileState.FromStaticFileData(staticFile);
                    contentRoutes.Add(outputFile.OutputPathRelative, outputFile);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to generate static content from file {staticFile.SourceFilePath}", ex);
                }
            }

            var posts = this.siteDataIndex.GetPosts();
            var postPages = new List<PageState>(posts.Count);
            foreach (var postData in posts)
            {
                try
                {
                    // Verify that the post's games are found in our metadata cache.
                    foreach (var game in postData.Games)
                    {
                        _ = this.siteDataIndex.ValidateMatchingGameName(game);
                    }

                    var page = PageState.FromPostData(this, postData);
                    postPages.Add(page);
                    contentRoutes.Add(page.OutputPathRelative, page);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to generate page from post {postData.SourceFilePath}", ex);
                }
            }

            var postsListPage = new PageState(this)
            {
                HideDate = true,
                Title = "Posts",
                PageType = "posts",
                Permalink = $"{this.GetBaseURL()}post/",
                OutputPathRelative = "post/index.html",
                RenderTemplateName = "list",
                LinkedPosts = posts,
            };
            contentRoutes.Add(postsListPage.OutputPathRelative, postsListPage);

            const int pagesPerHistoryPage = 10;
            for (var historyPageNum = 0; (historyPageNum * pagesPerHistoryPage) < postPages.Count; ++historyPageNum)
            {
                var historyPage = new PageState(this)
                {
                    HideDate = true,
                    HideTitle = true,
                    Permalink = this.GetBaseURL(), // BUG?: every history page has the BaseURL permalink!
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

                contentRoutes.Add(historyPage.OutputPathRelative, historyPage);
            }

            var rssFeedItems = Math.Min(postPages.Count, 15);
            var rssFeedPage = new PageState(this)
            {
                Date = posts[0].Date,
                OutputPathRelative = "index.xml",
                Permalink = this.GetBaseURL(),
                RenderTemplateName = "rss",
                HistoryPosts = postPages.GetRange(0, rssFeedItems),
            };
            contentRoutes.Add(rssFeedPage.OutputPathRelative, rssFeedPage);

            var categoriesIndex = new PageState(this)
            {
                HideDate = true,
                OutputPathRelative = "category/index.html",
                Permalink = $"{this.GetBaseURL()}category/",
                RenderTemplateName = "termslist",
                Title = "Categories",
                PageType = "categories",
                Terms = this.GetCategories(),
                TermsType = "category",
            };
            contentRoutes.Add(categoriesIndex.OutputPathRelative, categoriesIndex);

            var categories = this.siteDataIndex.GetCategories();
            foreach (var categoryData in categories)
            {
                var page = PageState.FromCategoryData(this, categoryData);
                contentRoutes.Add(page.OutputPathRelative, page);
            }

            var games = this.siteDataIndex.GetGames();
            var gamesIndex = new PageState(this)
            {
                HideDate = true,
                OutputPathRelative = "game/index.html",
                Permalink = $"{this.GetBaseURL()}game/",
                RenderTemplateName = "termslist",
                Title = "Games",
                PageType = "games",
                Terms = games.Select(g => g.Title).OrderBy(t => t, StringComparer.Ordinal).ToList(),
                TermsType = "game",
            };
            contentRoutes.Add(gamesIndex.OutputPathRelative, gamesIndex);

            foreach (var gameData in games)
            {
                var page = PageState.FromGameData(this, gameData);
                contentRoutes.Add(page.OutputPathRelative, page);
            }

            var platforms = this.siteDataIndex.GetPlatforms();
            var platformsIndex = new PageState(this)
            {
                HideDate = true,
                OutputPathRelative = "platform/index.html",
                Permalink = $"{this.GetBaseURL()}platform/",
                RenderTemplateName = "termslist",
                Title = "Platforms",
                PageType = "platforms",
                Terms = platforms.Select(p => p.Abbreviation).OrderBy(n => n, StringComparer.Ordinal).ToList(),
                TermsType = "platform",
            };
            contentRoutes.Add(platformsIndex.OutputPathRelative, platformsIndex);

            foreach (var platformData in platforms)
            {
                var page = PageState.FromPlatformData(this, platformData);
                contentRoutes.Add(page.OutputPathRelative, page);
            }

            var ratings = this.siteDataIndex.GetRatings();
            var ratingsIndex = new PageState(this)
            {
                HideDate = true,
                OutputPathRelative = "rating/index.html",
                Permalink = $"{this.GetBaseURL()}rating/",
                RenderTemplateName = "termslist",
                Title = "Ratings",
                PageType = "ratings",
                Terms = ratings.Select(r => r.Name).OrderBy(n => n).ToList(),
                TermsType = "rating",
            };
            contentRoutes.Add(ratingsIndex.OutputPathRelative, ratingsIndex);

            foreach (var ratingData in ratings)
            {
                var page = PageState.FromRatingData(this, ratingData);
                contentRoutes.Add(page.OutputPathRelative, page);
            }

            var tags = this.siteDataIndex.GetTags();
            var tagsIndex = new PageState(this)
            {
                HideDate = true,
                OutputPathRelative = "tag/index.html",
                Permalink = $"{this.GetBaseURL()}tag/",
                RenderTemplateName = "termslist",
                Title = "Tags",
                PageType = "tags",
                Terms = tags.Select(t => t.Name).OrderBy(n => n, StringComparer.Ordinal).ToList(),
                TermsType = "tag",
            };
            contentRoutes.Add(tagsIndex.OutputPathRelative, tagsIndex);

            foreach (var tagData in tags)
            {
                var page = PageState.FromTagData(this, tagData);
                contentRoutes.Add(page.OutputPathRelative, page);
            }

            var pages = this.siteDataIndex.GetPages();
            foreach (var pageData in pages)
            {
                var page = PageState.FromPageData(this, pageData);
                contentRoutes.Add(page.OutputPathRelative, page);
            }

            return contentRoutes;
        }
    }
}
