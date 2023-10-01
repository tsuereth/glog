using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.Data;
using GlogGenerator.TemplateRenderers;

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

        public string InputFilesBasePath { get; set; } = string.Empty;

        public string TemplateFilesBasePath { get; private set; } = "templates";

        private Antlr4.StringTemplate.TemplateGroupDirectory templateGroup;

        public SiteState(string inputFilesBasePath, string templateFilesBasePath)
        {
            this.InputFilesBasePath = inputFilesBasePath;
            this.TemplateFilesBasePath = templateFilesBasePath;
        }

        public Antlr4.StringTemplate.TemplateGroupDirectory GetTemplateGroup()
        {
            if (this.templateGroup == null)
            {
                // StringTemplate requires an absolute filepath.
                var templateFilesBasePath = this.TemplateFilesBasePath;
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

        public CategoryData AddCategoryIfMissing(string categoryName, bool overwriteData = false)
        {
            var categoryKey = StringRenderer.Urlize(categoryName);

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

        public PlatformData AddPlatformIfMissing(string platformName, bool overwriteData = false)
        {
            var platformKey = StringRenderer.Urlize(platformName);

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
            var ratingKey = StringRenderer.Urlize(ratingName);

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
            var tagKey = StringRenderer.Urlize(tagName);

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

        public GameData ValidateMatchingGameName(string gameName)
        {
            var gameNameUrlized = StringRenderer.Urlize(gameName);
            if (!this.Games.TryGetValue(gameNameUrlized, out var gameData))
            {
                throw new ArgumentException($"Game name \"{gameName}\" doesn't appear to exist in site state");
            }

            if (!gameData.Title.Equals(gameName, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Game name \"{gameName}\" doesn't exactly match game in site state \"{gameData.Title}\"");
            }

            return gameData;
        }

        public TagData ValidateMatchingTagName(string tagName)
        {
            var tagNameUrlized = StringRenderer.Urlize(tagName);
            if (!this.Tags.TryGetValue(tagNameUrlized, out var tagData))
            {
                throw new ArgumentException($"Tag name \"{tagName}\" doesn't appear to exist in site state");
            }

            if (!tagData.Name.Equals(tagName, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Tag name \"{tagName}\" doesn't exactly match tag in site state \"{tagData.Name}\"");
            }

            return tagData;
        }

        public void LoadContent()
        {
            // TODO: clear/reset page lists, route lists, et al.

            // Load game data from the IGDB cache.
            foreach (var igdbGame in this.IgdbCache.GetAllGames())
            {
                var gameData = GameData.FromIgdbGame(this.IgdbCache, igdbGame);

                var gameKey = StringRenderer.Urlize(gameData.Title);
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
            var staticBasePath = Path.Combine(this.InputFilesBasePath, StaticFileData.StaticContentBaseDir);
            var staticFilePaths = Directory.EnumerateFiles(staticBasePath, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var staticFilePath in staticFilePaths)
            {
                try
                {
                    var staticFile = StaticFileData.FromFilePath(staticFilePath);
                    var outputFile = StaticFileState.FromStaticFileData(this, staticFile);
                    this.ContentRoutes.Add(outputFile.OutputPathRelative, outputFile);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to load static content from {staticFilePath}", ex);
                }
            }

            var templateGroup = this.GetTemplateGroup();

            // Parse content to collect data.
            var postContentBasePath = Path.Combine(this.InputFilesBasePath, PostData.PostContentBaseDir);
            var postPaths = Directory.EnumerateFiles(postContentBasePath, "*.md", SearchOption.AllDirectories).ToList();

            var allPosts = new List<PostData>();
            foreach (var postPath in postPaths)
            {
                try
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
                        var gameUrlized = StringRenderer.Urlize(game);
                        var gameData = this.Games[gameUrlized];
                        gameData.LinkedPosts.Add(postData);

                        foreach (var tag in gameData.Tags)
                        {
                            var tagUrlized = StringRenderer.Urlize(tag);
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
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to load post from {postPath}", ex);
                }
            }

            // Now, we can render content.
            allPosts = allPosts.OrderByDescending(p => p.Date).ToList();
            var allPostPages = new List<PageState>(allPosts.Count);
            foreach (var postData in allPosts)
            {
                try
                {
                    var page = PageState.FromPostData(this, postData);
                    allPostPages.Add(page);
                    this.ContentRoutes.Add(page.OutputPathRelative, page);
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
                Permalink = $"{this.BaseURL}post/",
                OutputPathRelative = "post/index.html",
                RenderTemplateName = "list",
                LinkedPosts = allPosts,
            };
            this.ContentRoutes.Add(postsListPage.OutputPathRelative, postsListPage);

            const int pagesPerHistoryPage = 10;
            for (var historyPageNum = 0; (historyPageNum * pagesPerHistoryPage) < allPostPages.Count; ++historyPageNum)
            {
                var historyPage = new PageState(this)
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
            var rssFeedPage = new PageState(this)
            {
                Date = allPosts[0].Date,
                OutputPathRelative = "index.xml",
                Permalink = this.BaseURL,
                RenderTemplateName = "rss",
                HistoryPosts = allPostPages.GetRange(0, rssFeedItems),
            };
            this.ContentRoutes.Add(rssFeedPage.OutputPathRelative, rssFeedPage);

            var categoriesIndex = new PageState(this)
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

            var gamesIndex = new PageState(this)
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

            var platformsIndex = new PageState(this)
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

            var ratingsIndex = new PageState(this)
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

            var tagsIndex = new PageState(this)
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
                Path.Combine(this.InputFilesBasePath, "content", "backlog.md"),
                Path.Combine(this.InputFilesBasePath, "content", "upcoming.md"),
            };
            foreach (var pageFilePath in additionalPageFilePaths)
            {
                var pageData = PageData.FromFilePath(pageFilePath);
                var page = PageState.FromPageData(this, pageData);
                this.ContentRoutes.Add(page.OutputPathRelative, page);
            }
        }

        public static SiteState FromInputFilesBasePath(string inputFilesBasePath, string templateFilesBasePath)
        {
            var configFilePath = Path.Combine(inputFilesBasePath, "config.toml");
            var config = ConfigData.FromFilePath(configFilePath);

            var igdbCache = IgdbCache.FromJsonFile(inputFilesBasePath);

            var site = new SiteState(config.DataBasePath, templateFilesBasePath)
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
