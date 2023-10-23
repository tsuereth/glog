using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.RenderState;
using Microsoft.Extensions.Logging;

namespace GlogGenerator.Data
{
    public class SiteDataIndex
    {
        private readonly ILogger logger;
        private readonly string inputFilesBasePath;
        private readonly IgdbCache igdbCache;

        private Dictionary<string, CategoryData> categories = new Dictionary<string, CategoryData>();
        private Dictionary<string, GameData> games = new Dictionary<string, GameData>();
        private List<PageData> pages = new List<PageData>();
        private Dictionary<string, PlatformData> platforms = new Dictionary<string, PlatformData>();
        private List<PostData> posts = new List<PostData>();
        private Dictionary<string, RatingData> ratings = new Dictionary<string, RatingData>();
        private Dictionary<string, string> rawDataFiles = new Dictionary<string, string>();
        private List<StaticFileData> staticFiles = new List<StaticFileData>();
        private Dictionary<string, TagData> tags = new Dictionary<string, TagData>();

        public SiteDataIndex(
            ILogger logger,
            string inputFilesBasePath,
            IgdbCache igdbCache)
        {
            this.logger = logger;
            this.inputFilesBasePath = inputFilesBasePath;
            this.igdbCache = igdbCache;
        }

        public CategoryData AddCategoryIfMissing(string categoryName, bool overwriteData = false)
        {
            var categoryKey = UrlizedString.Urlize(categoryName);

            if (!this.categories.ContainsKey(categoryKey))
            {
                var newCategory = new CategoryData()
                {
                    Name = categoryName,
                };

                this.categories[categoryKey] = newCategory;
            }
            else if (overwriteData)
            {
                this.categories[categoryKey].Name = categoryName;
            }

            return this.categories[categoryKey];
        }

        public PlatformData AddPlatformIfMissing(string platformAbbreviation, bool overwriteData = false)
        {
            var platformKey = UrlizedString.Urlize(platformAbbreviation);

            if (!this.platforms.ContainsKey(platformKey))
            {
                var newPlatform = new PlatformData()
                {
                    Abbreviation = platformAbbreviation,
                };

                this.platforms[platformKey] = newPlatform;
            }
            else if (overwriteData)
            {
                this.platforms[platformKey].Abbreviation = platformAbbreviation;
            }

            return this.platforms[platformKey];
        }

        public RatingData AddRatingIfMissing(string ratingName, bool overwriteData = false)
        {
            var ratingKey = UrlizedString.Urlize(ratingName);

            if (!this.ratings.ContainsKey(ratingKey))
            {
                var newRating = new RatingData()
                {
                    Name = ratingName,
                };

                this.ratings[ratingKey] = newRating;
            }
            else if (overwriteData)
            {
                this.ratings[ratingKey].Name = ratingName;
            }

            return this.ratings[ratingKey];
        }

        public TagData AddTagIfMissing(string tagName, bool overwriteData = false)
        {
            var tagKey = UrlizedString.Urlize(tagName);

            if (!this.tags.ContainsKey(tagKey))
            {
                var newTag = new TagData()
                {
                    Name = tagName,
                };

                this.tags[tagKey] = newTag;
            }
            else if (overwriteData)
            {
                this.tags[tagKey].Name = tagName;
            }

            return this.tags[tagKey];
        }

        public List<CategoryData> GetCategories()
        {
            return this.categories.Values.ToList();
        }

        public GameData GetGame(string gameTitle)
        {
            if (this.games.TryGetValue(gameTitle, out var gameData))
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

        public List<TagData> GetTags()
        {
            return this.tags.Values.ToList();
        }

        public GameData ValidateMatchingGameName(string gameName)
        {
            var gameNameUrlized = UrlizedString.Urlize(gameName);
            if (!this.games.TryGetValue(gameNameUrlized, out var gameData))
            {
                throw new ArgumentException($"Game name \"{gameName}\" doesn't appear to exist in site data");
            }

            if (!gameData.Title.Equals(gameName, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Game name \"{gameName}\" doesn't exactly match game in site data \"{gameData.Title}\"");
            }

            return gameData;
        }

        public PlatformData ValidateMatchingPlatformAbbreviation(string platformAbbreviation)
        {
            var platformAbbreviationUrlized = UrlizedString.Urlize(platformAbbreviation);
            if (!this.platforms.TryGetValue(platformAbbreviationUrlized, out var platformData))
            {
                throw new ArgumentException($"Platform abbreviation \"{platformAbbreviation}\" doesn't appear to exist in site data");
            }

            if (!platformData.Abbreviation.Equals(platformAbbreviation, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Platform abbreviation \"{platformAbbreviation}\" doesn't exactly match platform in site data \"{platformData.Abbreviation}\"");
            }

            return platformData;
        }

        public TagData ValidateMatchingTagName(string tagName)
        {
            var tagNameUrlized = UrlizedString.Urlize(tagName);
            if (!this.tags.TryGetValue(tagNameUrlized, out var tagData))
            {
                throw new ArgumentException($"Tag name \"{tagName}\" doesn't appear to exist in site data");
            }

            if (!tagData.Name.Equals(tagName, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Tag name \"{tagName}\" doesn't exactly match tag in site data \"{tagData.Name}\"");
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

                var gameKey = UrlizedString.Urlize(gameData.Title);
                this.games[gameKey] = gameData;
            }

            // And platform data!
            foreach (var igdbPlatform in this.igdbCache.GetAllPlatforms())
            {
                var platformData = PlatformData.FromIgdbPlatform(this.igdbCache, igdbPlatform);

                var platformKey = UrlizedString.Urlize(platformData.Abbreviation);
                this.platforms[platformKey] = platformData;
            }

            // Prepare tags from game metadata.
            foreach (var gameData in this.games.Values)
            {
                foreach (var tag in gameData.Tags)
                {
                    this.AddTagIfMissing(tag);
                }
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
                        var categoryData = this.AddCategoryIfMissing(category, overwriteData: true);
                        categoryData.LinkedPosts.Add(postData);
                    }

                    var gameTagsByUrlized = new Dictionary<string, TagData>();
                    foreach (var game in postData.Games)
                    {
                        var gameUrlized = UrlizedString.Urlize(game);
                        var gameData = this.games[gameUrlized];
                        gameData.LinkedPosts.Add(postData);

                        foreach (var tag in gameData.Tags)
                        {
                            var tagUrlized = UrlizedString.Urlize(tag);
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
                        var platformData = this.AddPlatformIfMissing(platform, overwriteData: false);
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
