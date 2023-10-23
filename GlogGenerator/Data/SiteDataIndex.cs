using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.IgdbApi;
using GlogGenerator.RenderState;
using Microsoft.Extensions.Logging;

namespace GlogGenerator.Data
{
    public class SiteDataIndex
    {
        private readonly ILogger logger;
        private readonly string inputFilesBasePath;
        private readonly IgdbCache igdbCache;

        private Dictionary<UrlizedString, CategoryData> categories = new Dictionary<UrlizedString, CategoryData>();
        private Dictionary<UrlizedString, GameData> games = new Dictionary<UrlizedString, GameData>();
        private List<PageData> pages = new List<PageData>();
        private Dictionary<UrlizedString, PlatformData> platforms = new Dictionary<UrlizedString, PlatformData>();
        private List<PostData> posts = new List<PostData>();
        private Dictionary<UrlizedString, RatingData> ratings = new Dictionary<UrlizedString, RatingData>();
        private Dictionary<string, string> rawDataFiles = new Dictionary<string, string>();
        private List<StaticFileData> staticFiles = new List<StaticFileData>();
        private Dictionary<UrlizedString, TagData> tags = new Dictionary<UrlizedString, TagData>();

        public SiteDataIndex(
            ILogger logger,
            string inputFilesBasePath,
            IgdbCache igdbCache)
        {
            this.logger = logger;
            this.inputFilesBasePath = inputFilesBasePath;
            this.igdbCache = igdbCache;
        }

        public CategoryData AddCategoryIfMissing(string categoryName)
        {
            var categoryNameUrlized = new UrlizedString(categoryName);
            if (!this.categories.TryGetValue(categoryNameUrlized, out var categoryData))
            {
                categoryData = new CategoryData()
                {
                    Name = categoryName,
                };

                this.categories[categoryNameUrlized] = categoryData;
            }

            return categoryData;
        }

        public RatingData AddRatingIfMissing(string ratingName)
        {
            var ratingNameUrlized = new UrlizedString(ratingName);
            if (!this.ratings.TryGetValue(ratingNameUrlized, out var ratingData))
            {
                ratingData = new RatingData()
                {
                    Name = ratingName,
                };

                this.ratings[ratingNameUrlized] = ratingData;
            }

            return ratingData;
        }

        public List<CategoryData> GetCategories()
        {
            return this.categories.Values.ToList();
        }

        public GameData GetGame(string gameTitle)
        {
            var gameTitleUrlized = new UrlizedString(gameTitle);
            if (this.games.TryGetValue(gameTitleUrlized, out var gameData))
            {
                return gameData;
            }

            return null;
        }

        public List<GameData> GetGames()
        {
            return this.games.Values.ToList();
        }

        public List<PageData> GetPages()
        {
            return this.pages;
        }

        public PlatformData GetPlatform(string platformAbbreviation)
        {
            var platformAbbreviationUrlized = new UrlizedString(platformAbbreviation);
            if (this.platforms.TryGetValue(platformAbbreviationUrlized, out var platformData))
            {
                return platformData;
            }

            return null;
        }

        public List<PlatformData> GetPlatforms()
        {
            return this.platforms.Values.ToList();
        }

        public List<PostData> GetPosts()
        {
            return this.posts;
        }

        public List<RatingData> GetRatings()
        {
            return this.ratings.Values.ToList();
        }

        public string GetRawDataFile(string filePath)
        {
            if (this.rawDataFiles.TryGetValue(filePath, out var rawDataFile))
            {
                return rawDataFile;
            }

            return null;
        }

        public List<StaticFileData> GetStaticFiles()
        {
            return this.staticFiles;
        }

        public TagData GetTag(string tagName)
        {
            var tagNameUrlized = new UrlizedString(tagName);
            if (this.tags.TryGetValue(tagNameUrlized, out var tagData))
            {
                return tagData;
            }

            return null;
        }

        public List<TagData> GetTags()
        {
            return this.tags.Values.ToList();
        }

        public GameData ValidateMatchingGameName(string gameName)
        {
            var gameNameUrlized = new UrlizedString(gameName);
            if (!this.games.TryGetValue(gameNameUrlized, out var gameData))
            {
                throw new ArgumentException($"Game name \"{gameName}\" doesn't appear to exist in site data");
            }

            return gameData;
        }

        public PlatformData ValidateMatchingPlatformAbbreviation(string platformAbbreviation)
        {
            var platformAbbreviationUrlized = new UrlizedString(platformAbbreviation);
            if (!this.platforms.TryGetValue(platformAbbreviationUrlized, out var platformData))
            {
                throw new ArgumentException($"Platform abbreviation \"{platformAbbreviation}\" doesn't appear to exist in site data");
            }

            return platformData;
        }

        public TagData ValidateMatchingTagName(string tagName)
        {
            var tagNameUrlized = new UrlizedString(tagName);
            if (!this.tags.TryGetValue(tagNameUrlized, out var tagData))
            {
                throw new ArgumentException($"Tag name \"{tagName}\" doesn't appear to exist in site data");
            }

            return tagData;
        }

        public void LoadContent()
        {
            this.categories.Clear();
            this.games.Clear();
            this.pages.Clear();
            this.platforms.Clear();
            this.posts.Clear();
            this.ratings.Clear();
            this.rawDataFiles.Clear();
            this.staticFiles.Clear();
            this.tags.Clear();

            // Load game data from the IGDB cache.
            foreach (var igdbGame in this.igdbCache.GetAllGames())
            {
                var gameData = GameData.FromIgdbGame(this.igdbCache, igdbGame);

                var gameTitleUrlized = new UrlizedString(gameData.Title);
                this.games.Add(gameTitleUrlized, gameData);
            }

            // And platform data!
            foreach (var igdbPlatform in this.igdbCache.GetAllPlatforms())
            {
                var platformData = PlatformData.FromIgdbPlatform(this.igdbCache, igdbPlatform);

                var platformAbbreviationUrlized = new UrlizedString(platformData.Abbreviation);
                this.platforms.Add(platformAbbreviationUrlized, platformData);
            }

            // Prepare tags from game metadata.
            foreach (var igdbGameCategory in (IgdbGameCategory[])Enum.GetValues(typeof(IgdbGameCategory)))
            {
                if (igdbGameCategory == IgdbGameCategory.None)
                {
                    continue;
                }

                var tagName = igdbGameCategory.Description();

                var tagData = new TagData()
                {
                    Name = tagName,
                };

                var tagNameUrlized = new UrlizedString(tagName);
                this.tags.Add(tagNameUrlized, tagData);
            }

            foreach (var igdbGameMetadata in this.igdbCache.GetAllGameMetadata())
            {
                var tagName = igdbGameMetadata.GetReferenceableKey();
                var tagNameUrlized = new UrlizedString(tagName);
                if (this.tags.TryGetValue(tagNameUrlized, out var tagData))
                {
                    this.logger.LogError("Metadata conflict! existing tag {ExistingTagName} would conflict with additional tag {NewTagName}",
                        tagData.Name,
                        tagName);

                    continue;
                }

                tagData = new TagData()
                {
                    Name = tagName,
                };

                this.tags.Add(tagNameUrlized, tagData);
            }

            // List raw data files.
            var rawDataBasePath = Path.Combine(this.inputFilesBasePath, "data");
            var rawDataFilePaths = Directory.EnumerateFiles(rawDataBasePath, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var rawDataFilePath in rawDataFilePaths)
            {
                try
                {
                    var rawDataFile = File.ReadAllText(rawDataFilePath);

                    var relativePathParts = rawDataFilePath.GetPathPartsWithStartingDirName("data");
                    var relativePath = string.Join('/', relativePathParts);
                    this.rawDataFiles[relativePath] = rawDataFile;
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to load raw data from {rawDataFilePath}", ex);
                }
            }

            // List static content.
            var staticBasePath = Path.Combine(this.inputFilesBasePath, StaticFileData.StaticContentBaseDir);
            var staticFilePaths = Directory.EnumerateFiles(staticBasePath, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var staticFilePath in staticFilePaths)
            {
                try
                {
                    var staticFile = StaticFileData.FromFilePath(staticFilePath);
                    this.staticFiles.Add(staticFile);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to load static content from {staticFilePath}", ex);
                }
            }

            // Parse content to collect data.
            var postContentBasePath = Path.Combine(this.inputFilesBasePath, PostData.PostContentBaseDir);
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
                        var categoryData = this.AddCategoryIfMissing(category);
                        categoryData.LinkedPosts.Add(postData);
                    }

                    var postGameTags = new Dictionary<string, TagData>();
                    foreach (var game in postData.Games)
                    {
                        var gameData = this.GetGame(game);
                        gameData.LinkedPosts.Add(postData);

                        foreach (var tag in gameData.Tags)
                        {
                            postGameTags[tag] = this.GetTag(tag);
                        }
                    }

                    foreach (var tagData in postGameTags.Values)
                    {
                        tagData.LinkedPosts.Add(postData);
                    }

                    foreach (var platform in postData.Platforms)
                    {
                        var platformData = this.GetPlatform(platform);
                        platformData.LinkedPosts.Add(postData);
                    }

                    foreach (var rating in postData.Ratings)
                    {
                        var ratingData = this.AddRatingIfMissing(rating);
                        ratingData.LinkedPosts.Add(postData);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed to load post from {postPath}", ex);
                }
            }
            this.posts = allPosts.OrderByDescending(p => p.Date).ToList();

            var additionalPageFilePaths = new List<string>()
            {
                Path.Combine(this.inputFilesBasePath, "content", "backlog.md"),
                Path.Combine(this.inputFilesBasePath, "content", "upcoming.md"),
            };
            foreach (var pageFilePath in additionalPageFilePaths)
            {
                var pageData = PageData.FromFilePath(pageFilePath);
                this.pages.Add(pageData);
            }
        }
    }
}
