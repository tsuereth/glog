using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GlogGenerator.Data;
using GlogGenerator.IgdbApi;
using GlogGenerator.MarkdownExtensions;
using GlogGenerator.RenderState;
using GlogGenerator.Stats;
using Markdig;
using Markdig.Extensions.ListExtras;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlogGenerator
{
    public class SiteBuilder : ISiteBuilder
    {
        private const string VariableNameSiteBaseURL = "SiteBaseURL";

        private readonly ILogger logger;
        private readonly ConfigData configData;

        private Mode mode;
        private IncludeDrafts includeDrafts;
        private DateTimeOffset buildDate;
        private ISiteDataIndex siteDataIndex;
        private SiteState siteState;
        private ContentParser contentParser;

        private IgdbCache igdbCache = null;

        public SiteBuilder() : this(
            NullLogger.Instance,
            new ConfigData()) { }

        public SiteBuilder(
            ILogger logger,
            ConfigData configData,
            ISiteDataIndex siteDataIndex = null)
        {
            this.logger = logger;
            this.configData = configData;

            this.mode = Mode.Build;
            this.includeDrafts = IncludeDrafts.Never;
            this.buildDate = DateTimeOffset.Now;

            if (siteDataIndex != null)
            {
                this.siteDataIndex = siteDataIndex;
            }
            else
            {
                this.siteDataIndex = new SiteDataIndex(this.logger, this.configData.InputFilesBasePath);
            }

            this.siteState = new SiteState(this, this.configData.TemplateFilesBasePath);

            var renderVariableSubstitution = new VariableSubstitution();
            renderVariableSubstitution.SetSubstitution(VariableNameSiteBaseURL, this.configData.BaseURL);
            this.contentParser = new ContentParser(renderVariableSubstitution, this.siteDataIndex, this.siteState);
        }

        private IIgdbCache GetIgdbCache()
        {
            if (this.igdbCache == null)
            {
                this.igdbCache = IgdbCache.FromJsonFiles(this.configData.InputFilesBasePath);
            }

            return this.igdbCache;
        }

        public void SetMode(Mode mode, IncludeDrafts includeDrafts)
        {
            this.mode = mode;
            this.includeDrafts = includeDrafts;
        }

        public DateTimeOffset GetBuildDate()
        {
            return this.buildDate;
        }

        public void SetBuildDate(DateTimeOffset buildDate)
        {
            this.buildDate = buildDate;
        }

        public void LoadSiteDataIndexFiles()
        {
            this.siteDataIndex.LoadFromJsonFiles(
                this.configData.SiteDataIndexFilesBasePath,
                this.configData.InputFilesBasePath,
                this.GetContentParser());
        }

        public void RewriteSiteDataIndexFiles()
        {
            this.siteDataIndex.WriteToJsonFiles(this.configData.SiteDataIndexFilesBasePath);
        }

        public ContentParser GetContentParser()
        {
            return this.contentParser;
        }

        public bool TryLoadIgdbCache()
        {
            if (IgdbCache.JsonFilesExist(this.configData.InputFilesBasePath))
            {
                this.igdbCache = IgdbCache.FromJsonFiles(this.configData.InputFilesBasePath);
                return true;
            }

            return false;
        }

        public void SetAdditionalIgdbGameIds(List<int> igdbGameIds)
        {
            this.siteDataIndex.SetAdditionalIgdbGameIds(igdbGameIds);
        }

        public void RewriteIgdbCache()
        {
            var igdbCache = this.GetIgdbCache();
            igdbCache.WriteToJsonFiles(this.configData.InputFilesBasePath);
        }

        public async Task UpdateIgdbCacheFromApiAsync(IgdbApiClient apiClient)
        {
            var igdbCache = this.GetIgdbCache();
            await igdbCache.UpdateFromApiClient(apiClient);

            igdbCache.WriteToJsonFiles(this.configData.InputFilesBasePath);
        }

        public void UpdateDataIndex()
        {
            // "Now Playing" game references, outside of content files, may not otherwise be known to the data index.
            // Make sure that the index knows those game names when it's loading and handling data references.
            this.siteDataIndex.SetNonContentGameNames(this.GetNowPlaying());

            var igdbCache = this.GetIgdbCache();

            var dataIncludesDrafts =
                (this.includeDrafts == IncludeDrafts.Always) ||
                (this.mode == Mode.Host && this.includeDrafts == IncludeDrafts.HostModeOnly);

            this.siteDataIndex.LoadContent(igdbCache, this.contentParser, dataIncludesDrafts);
        }

        public void ResolveDataReferences()
        {
            this.siteDataIndex.ResolveReferences();
            this.siteDataIndex.RegisterAutomaticReferences(this.igdbCache);
            this.siteDataIndex.RemoveUnreferencedData(this.igdbCache);
            this.siteDataIndex.LinkPostsToAssociatedGames();
        }

        public List<PageData> GetPages()
        {
            return this.siteDataIndex.GetPages();
        }

        public List<PostData> GetPosts()
        {
            return this.siteDataIndex.GetPosts();
        }

        public void RewriteData()
        {
            this.siteDataIndex.RewriteSourceContent(this.contentParser);
        }

        public List<GameStats> GetGameStatsForDateRange(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var reportPosts = this.siteDataIndex.GetPosts()
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .Where(p => p.IsInPlayingAGameCategory())
                .ToList();

            var statsByGameAndPlatform = new Dictionary<string, GameStats>();
            foreach (var reportPost in reportPosts)
            {
                if (reportPost.Games != null)
                {
                    foreach (var postGameReference in reportPost.Games)
                    {
                        var postGameData = this.siteDataIndex.GetData(postGameReference);

                        if (reportPost.Platforms != null)
                        {
                            foreach (var postPlatformReference in reportPost.Platforms)
                            {
                                var postPlatformData = this.siteDataIndex.GetData(postPlatformReference);

                                var gameAndPlatformKey = $"{postGameData.GetDataId()}:{postPlatformData.GetDataId()}";

                                if (!statsByGameAndPlatform.ContainsKey(gameAndPlatformKey))
                                {
                                    statsByGameAndPlatform[gameAndPlatformKey] = new GameStats()
                                    {
                                        Title = postGameData.Title,
                                        Platform = postPlatformData.Abbreviation,
                                        Type = postGameData.GameType,
                                        FirstPosted = reportPost.Date,
                                        LastPosted = reportPost.Date,
                                    };
                                }

                                if (reportPost.Date < statsByGameAndPlatform[gameAndPlatformKey].FirstPosted)
                                {
                                    statsByGameAndPlatform[gameAndPlatformKey].FirstPosted = reportPost.Date;
                                }

                                if (reportPost.Date > statsByGameAndPlatform[gameAndPlatformKey].LastPosted)
                                {
                                    statsByGameAndPlatform[gameAndPlatformKey].LastPosted = reportPost.Date;
                                }

                                if (reportPost.Ratings != null && reportPost.Ratings.Count > 0)
                                {
                                    var postRatingReference = reportPost.Ratings[0];
                                    var postRatingData = this.siteDataIndex.GetData(postRatingReference);

                                    statsByGameAndPlatform[gameAndPlatformKey].Rating = postRatingData.Name;
                                }

                                ++statsByGameAndPlatform[gameAndPlatformKey].NumPosts;
                            }
                        }
                    }
                }
            }

            var stats = statsByGameAndPlatform.Values.OrderBy(s => s.FirstPosted).ToList();
            return stats;
        }

        public void UpdateContentRoutes()
        {
            this.siteState.LoadContentRoutes();
        }

        public SiteState GetSiteState()
        {
            return this.siteState;
        }

        public string GetBaseURL()
        {
            if (!this.contentParser.GetVariableSubstitution().TryGetSubstitution(VariableNameSiteBaseURL, out var baseURL))
            {
                // Since this variable is set in the constructor, it should never, ever be missing.
                throw new InvalidDataException();
            }

            return baseURL;
        }

        public void SetBaseURL(string baseURL)
        {
            this.contentParser.GetVariableSubstitution().SetSubstitution(VariableNameSiteBaseURL, baseURL);
        }

        public List<string> GetCategories()
        {
            return this.siteDataIndex.GetCategories()?.Select(c => c.Name).OrderBy(c => c).ToList();
        }

        public List<string> GetNowPlaying()
        {
            return this.configData.NowPlaying;
        }

        public VariableSubstitution GetVariableSubstitution()
        {
            return this.contentParser.GetVariableSubstitution();
        }

        public HtmlRendererContext GetRendererContext()
        {
            return this.contentParser.GetGlogMarkdownExtension().GetRendererContext();
        }

        public string RenderHtml(MarkdownDocument parsedDocument)
        {
            return parsedDocument.ToHtml(this.contentParser.GetHtmlRenderPipeline());
        }

        public Dictionary<string, IOutputContent> ResolveContentRoutes()
        {
            var contentRoutes = new Dictionary<string, IOutputContent>();

            var staticFiles = this.siteDataIndex.GetStaticFiles();
            if (staticFiles != null)
            {
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
            }

            var posts = this.siteDataIndex.GetPosts();
            var postPages = new List<PageState>(posts?.Count ?? 0);
            if (posts != null)
            {
                foreach (var postData in posts)
                {
                    try
                    {
                        if (postData.Games != null)
                        {
                            // Verify that the post's games are found in our metadata cache.
                            foreach (var gameReference in postData.Games)
                            {
                                _ = this.siteDataIndex.GetData(gameReference);
                            }
                        }

                        var page = PageState.FromPostData(this, postData, this.siteDataIndex);
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
                    LinkedPosts = posts.Select(p => LinkedPostProperties.FromPostData(p, this.siteDataIndex)).ToList(),
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
            }

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
            if (categories != null)
            {
                foreach (var categoryData in categories)
                {
                    var page = PageState.FromCategoryData(this, categoryData, this.siteDataIndex);
                    contentRoutes.Add(page.OutputPathRelative, page);
                }
            }

            var games = this.siteDataIndex.GetGames();
            if (games != null)
            {
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
                    var page = PageState.FromGameData(this, gameData, this.siteDataIndex);
                    contentRoutes.Add(page.OutputPathRelative, page);
                }
            }

            var platforms = this.siteDataIndex.GetPlatforms();
            if (platforms != null)
            {
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
                    var page = PageState.FromPlatformData(this, platformData, this.siteDataIndex);
                    contentRoutes.Add(page.OutputPathRelative, page);
                }
            }

            var ratings = this.siteDataIndex.GetRatings();
            if (ratings != null)
            {
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
                    var page = PageState.FromRatingData(this, ratingData, this.siteDataIndex);
                    contentRoutes.Add(page.OutputPathRelative, page);
                }
            }

            var tags = this.siteDataIndex.GetTags();
            if (tags != null)
            {
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
                    var page = PageState.FromTagData(this, tagData, this.siteDataIndex);
                    contentRoutes.Add(page.OutputPathRelative, page);
                }
            }

            var pages = this.siteDataIndex.GetPages();
            if (pages != null)
            {
                foreach (var pageData in pages)
                {
                    var page = PageState.FromPageData(this, pageData);
                    contentRoutes.Add(page.OutputPathRelative, page);
                }
            }

            return contentRoutes;
        }

        public enum Mode
        {
            Build,
            Host,
            ReportStats,
            UpdateDataNoOutput,
        }

        public enum IncludeDrafts
        {
            Never,
            Always,
            HostModeOnly,
        }
    }
}
