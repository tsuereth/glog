using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.Data;
using GlogGenerator.HugoCompat;

namespace GlogGenerator.RenderState
{
    public class SiteState
    {
        public string Author { get; set; } = string.Empty;

        public string BaseURL { get; set; } = string.Empty;

        public DateTimeOffset BuildDate { get; set; } = DateTimeOffset.MinValue;

        public Dictionary<string, CategoryData> Categories { get; set; } = new Dictionary<string, CategoryData>();

        public List<string> CategoriesSorted
        {
            get
            {
                return this.Categories.Values.Select(c => c.Name).OrderBy(c => c).ToList();
            }
        }

        public Dictionary<string, GameData> Games { get; set; } = new Dictionary<string, GameData>();

        public string LanguageCode { get; set; } = string.Empty;

        public List<string> NowPlaying { get; set; } = new List<string>();

        public Dictionary<string, PlatformData> Platforms { get; set; } = new Dictionary<string, PlatformData>();

        public Dictionary<string, RatingData> Ratings { get; set; } = new Dictionary<string, RatingData>();

        public Dictionary<string, TagData> Tags { get; set; } = new Dictionary<string, TagData>();

        public string Title { get; set; } = string.Empty;

        public Dictionary<string, IOutputContent> ContentRoutes { get; set; } = new Dictionary<string, IOutputContent>();

        public IgdbCache IgdbCache { get; set; }

        public FilePathResolver PathResolver { get; private set; }

        public string TemplatesBasePath { get; private set; } = "templates";

        private Antlr4.StringTemplate.TemplateGroupDirectory templateGroup;

        public SiteState(string basePath, string templatesBasePath)
        {
            this.PathResolver = new FilePathResolver(basePath);
            this.TemplatesBasePath = templatesBasePath;
        }

        public Antlr4.StringTemplate.TemplateGroupDirectory GetTemplateGroup()
        {
            if (this.templateGroup == null)
            {
                // StringTemplate requires an absolute filepath.
                var templatesBasePath = this.TemplatesBasePath;
                if (!Path.IsPathRooted(templatesBasePath))
                {
                    templatesBasePath = Path.GetFullPath(templatesBasePath);
                }

                this.templateGroup = new Antlr4.StringTemplate.TemplateGroupDirectory(
                    templatesBasePath,
                    delimiterStartChar: '%',
                    delimiterStopChar: '%');

                this.templateGroup.RegisterRenderer(typeof(DateTimeOffset), new TemplateFunctionsDateTimeRenderer());
                this.templateGroup.RegisterRenderer(typeof(string), new TemplateFunctionsStringRenderer());
            }

            return this.templateGroup;
        }

        public CategoryData AddCategoryIfMissing(string categoryName, bool overwriteData = false)
        {
            var categoryKey = TemplateFunctionsStringRenderer.Urlize(categoryName);

            if (!this.Categories.ContainsKey(categoryKey))
            {
                var newCategory = new CategoryData()
                {
                    Name = categoryName,
                };

                this.Categories[categoryKey] = newCategory;
            }
            else if (overwriteData)
            {
                this.Categories[categoryKey].Name = categoryName;
            }

            return this.Categories[categoryKey];
        }

        public GameData AddGameIfMissing(string gameName, bool overwriteData = false)
        {
            var gameKey = TemplateFunctionsStringRenderer.Urlize(gameName);

            if (!this.Games.ContainsKey(gameKey))
            {
                var newGame = new GameData()
                {
                    Title = gameName,
                };

                this.Games[gameKey] = newGame;
            }
            else if (overwriteData)
            {
                this.Games[gameKey].Title = gameName;
            }

            return this.Games[gameKey];
        }

        public PlatformData AddPlatformIfMissing(string platformName, bool overwriteData = false)
        {
            var platformKey = TemplateFunctionsStringRenderer.Urlize(platformName);

            if (!this.Platforms.ContainsKey(platformKey))
            {
                var newPlatform = new PlatformData()
                {
                    Name = platformName,
                };

                this.Platforms[platformKey] = newPlatform;
            }
            else if (overwriteData)
            {
                this.Platforms[platformKey].Name = platformName;
            }

            return this.Platforms[platformKey];
        }

        public RatingData AddRatingIfMissing(string ratingName, bool overwriteData = false)
        {
            var ratingKey = TemplateFunctionsStringRenderer.Urlize(ratingName);

            if (!this.Ratings.ContainsKey(ratingKey))
            {
                var newRating = new RatingData()
                {
                    Name = ratingName,
                };

                this.Ratings[ratingKey] = newRating;
            }
            else if (overwriteData)
            {
                this.Ratings[ratingKey].Name = ratingName;
            }

            return this.Ratings[ratingKey];
        }

        public TagData AddTagIfMissing(string tagName, bool overwriteData = false)
        {
            var tagKey = TemplateFunctionsStringRenderer.Urlize(tagName);

            if (!this.Tags.ContainsKey(tagKey))
            {
                var newTag = new TagData()
                {
                    Name = tagName,
                };

                this.Tags[tagKey] = newTag;
            }
            else if (overwriteData)
            {
                this.Tags[tagKey].Name = tagName;
            }

            return this.Tags[tagKey];
        }

        public void LoadContent()
        {
            // TODO: clear/reset page lists, route lists, et al.

            // Load game data from the IGDB cache.
            foreach (var igdbGame in this.IgdbCache.GetAllGames())
            {
                var gameData = GameData.FromIgdbGame(this.IgdbCache, igdbGame);

                var gameKey = TemplateFunctionsStringRenderer.Urlize(gameData.Title);
                this.Games[gameKey] = gameData;
            }

            // Prepare tags from game metadata.
            foreach (var gameData in this.Games.Values)
            {
                foreach (var tag in gameData.Tags)
                {
                    this.AddTagIfMissing(tag);
                }
            }

            // List static content.
            var staticBasePath = Path.Combine(this.PathResolver.BasePath, StaticFileData.StaticContentBaseDir);
            var staticFilePaths = Directory.EnumerateFiles(staticBasePath, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var staticFilePath in staticFilePaths)
            {
                var staticFile = StaticFileData.FromFilePath(staticFilePath);
                var outputFile = StaticFileState.FromStaticFileData(this, staticFile);
                this.ContentRoutes.Add(outputFile.OutputPathRelative, outputFile);
            }

            var templateGroup = this.GetTemplateGroup();

            // Parse content to collect data.
            var postContentBasePath = Path.Combine(this.PathResolver.BasePath, PostData.PostContentBaseDir);
            var postPaths = Directory.EnumerateFiles(postContentBasePath, "*.md", SearchOption.AllDirectories).ToList();

            var allPosts = new List<PostData>();
            foreach (var postPath in postPaths)
            {
                var postData = PostData.FromFilePath(postPath);

                if (postData.Draft)
                {
                    continue;
                }

                allPosts.Add(postData);

                foreach (var category in postData.Categories)
                {
                    var categoryData = this.AddCategoryIfMissing(category, overwriteData: true);
                    categoryData.LinkedPosts.Add(postData);
                }

                var gameTagsByUrlized = new Dictionary<string, TagData>();
                foreach (var game in postData.Games)
                {
                    // Quirk note: a "better" source of GameData has already been processed
                    // (from game files directly), so don't allow post metadata to overwrite that.
                    var gameData = this.AddGameIfMissing(game, overwriteData: false);
                    gameData.LinkedPosts.Add(postData);

                    foreach (var tag in gameData.Tags)
                    {
                        var tagUrlized = TemplateFunctionsStringRenderer.Urlize(tag);
                        var tagData = this.AddTagIfMissing(tag, overwriteData: false);
                        gameTagsByUrlized[tagUrlized] = tagData;
                    }
                }

                foreach (var tagData in gameTagsByUrlized.Values)
                {
                    tagData.LinkedPosts.Add(postData);
                }

                foreach (var platform in postData.Platforms)
                {
                    var platformData = this.AddPlatformIfMissing(platform, overwriteData: true);
                    platformData.LinkedPosts.Add(postData);
                }

                foreach (var rating in postData.Ratings)
                {
                    var ratingData = this.AddRatingIfMissing(rating, overwriteData: true);
                    ratingData.LinkedPosts.Add(postData);
                }

                // We need to run shortcode parsing to detect all games, tags, etc.
                _ = PageState.FromPostData(this, postData);
            }

            allPosts = allPosts.OrderByDescending(p => p.Date).ToList();

            // Now, we can render content.
            var allPostPages = new List<PageState>(allPosts.Count);
            foreach (var postData in allPosts)
            {
                var page = PageState.FromPostData(this, postData);
                allPostPages.Add(page);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var postsListPage = new PageState()
            {
                HideDate = true,
                Title = "Posts",
                PageType = "posts",
                Permalink = $"{this.BaseURL}post/",
                OutputPathRelative = "post/index.html",
                RenderTemplateName = "list",
                LinkedPosts = allPosts,
            };
            this.ContentRoutes.Add(postsListPage.OutputPathRelative, postsListPage);

            const int pagesPerHistoryPage = 10;
            for (var historyPageNum = 0; (historyPageNum * pagesPerHistoryPage) < allPostPages.Count; ++historyPageNum)
            {
                var historyPage = new PageState()
                {
                    HideDate = true,
                    HideTitle = true,
                    Permalink = this.BaseURL, // BUG?: every history page has the BaseURL permalink!
                    RenderTemplateName = "history",
                };

                var pageOneBased = historyPageNum + 1;

                var firstPostNum = historyPageNum * pagesPerHistoryPage;
                var postsCount = pagesPerHistoryPage;
                if (firstPostNum + postsCount >= allPostPages.Count)
                {
                    postsCount = allPostPages.Count - firstPostNum;
                }

                historyPage.HistoryPosts = allPostPages.GetRange(firstPostNum, postsCount);

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

                if (((historyPageNum + 1) * pagesPerHistoryPage) >= allPostPages.Count)
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

            var rssFeedItems = Math.Min(allPostPages.Count, 15);
            var rssFeedPage = new PageState()
            {
                Date = allPosts[0].Date,
                OutputPathRelative = "index.xml",
                Permalink = this.BaseURL,
                RenderTemplateName = "rss",
                HistoryPosts = allPostPages.GetRange(0, rssFeedItems),
            };
            this.ContentRoutes.Add(rssFeedPage.OutputPathRelative, rssFeedPage);

            var categoriesIndex = new PageState()
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

            foreach (var categoryData in this.Categories.Values)
            {
                var page = PageState.FromCategoryData(this, categoryData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var gamesIndex = new PageState()
            {
                HideDate = true,
                OutputPathRelative = "game/index.html",
                Permalink = $"{this.BaseURL}game/",
                RenderTemplateName = "termslist",
                Title = "Games",
                PageType = "games",
                Terms = this.Games.Values.Select(g => g.Title).OrderBy(t => t, StringComparer.Ordinal).ToList(),
                TermsType = "game",
            };
            this.ContentRoutes.Add(gamesIndex.OutputPathRelative, gamesIndex);

            foreach (var gameData in this.Games.Values)
            {
                var page = PageState.FromGameData(this, gameData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var platformsIndex = new PageState()
            {
                HideDate = true,
                OutputPathRelative = "platform/index.html",
                Permalink = $"{this.BaseURL}platform/",
                RenderTemplateName = "termslist",
                Title = "Platforms",
                PageType = "platforms",
                Terms = this.Platforms.Values.Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToList(),
                TermsType = "platform",
            };
            this.ContentRoutes.Add(platformsIndex.OutputPathRelative, platformsIndex);

            foreach (var platformData in this.Platforms.Values)
            {
                var page = PageState.FromPlatformData(this, platformData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var ratingsIndex = new PageState()
            {
                HideDate = true,
                OutputPathRelative = "rating/index.html",
                Permalink = $"{this.BaseURL}rating/",
                RenderTemplateName = "termslist",
                Title = "Ratings",
                PageType = "ratings",
                Terms = this.Ratings.Values.Select(r => r.Name).OrderBy(n => n).ToList(),
                TermsType = "rating",
            };
            this.ContentRoutes.Add(ratingsIndex.OutputPathRelative, ratingsIndex);

            foreach (var ratingData in this.Ratings.Values)
            {
                var page = PageState.FromRatingData(this, ratingData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var tagsIndex = new PageState()
            {
                HideDate = true,
                OutputPathRelative = "tag/index.html",
                Permalink = $"{this.BaseURL}tag/",
                RenderTemplateName = "termslist",
                Title = "Tags",
                PageType = "tags",
                Terms = this.Tags.Values.Select(t => t.Name).OrderBy(n => n, StringComparer.Ordinal).ToList(),
                TermsType = "tag",
            };
            this.ContentRoutes.Add(tagsIndex.OutputPathRelative, tagsIndex);

            foreach (var tagData in this.Tags.Values)
            {
                var page = PageState.FromTagData(this, tagData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }

            var additionalPageFilePaths = new List<string>()
            {
                Path.Combine(this.PathResolver.BasePath, "content", "backlog.md"),
                Path.Combine(this.PathResolver.BasePath, "content", "upcoming.md"),
            };
            foreach (var pageFilePath in additionalPageFilePaths)
            {
                var pageData = PageData.FromFilePath(pageFilePath);
                var page = PageState.FromPageData(this, pageData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }
        }

        public static SiteState FromInputFilesBasePath(string inputFilesBasePath, string templatesBasePath)
        {
            var configFilePath = Path.Combine(inputFilesBasePath, "config.toml");
            var config = ConfigData.FromFilePath(configFilePath);

            var igdbCacheFilesDirectory = Path.Combine(inputFilesBasePath, IgdbCache.JsonFilesBaseDir);
            var igdbCache = IgdbCache.FromJsonFiles(igdbCacheFilesDirectory);

            var site = new SiteState(config.DataBasePath, templatesBasePath)
            {
                Author = config.Author,
                BaseURL = config.BaseURL,
                BuildDate = DateTimeOffset.Now,
                LanguageCode = config.LanguageCode,
                NowPlaying = config.NowPlaying,
                Title = config.Title,

                IgdbCache = igdbCache,
            };

            return site;
        }
    }
}
