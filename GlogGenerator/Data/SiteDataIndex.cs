using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.IgdbApi;
using GlogGenerator.RenderState;
using Microsoft.Extensions.Logging;

namespace GlogGenerator.Data
{
    public class SiteDataIndex: ISiteDataIndex
    {
        private readonly ILogger logger;
        private readonly ISiteBuilder siteBuilder;
        private readonly string inputFilesBasePath;

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
            ISiteBuilder siteBuilder,
            string inputFilesBasePath)
        {
            this.logger = logger;
            this.siteBuilder = siteBuilder;
            this.inputFilesBasePath = inputFilesBasePath;
        }

        private CategoryData AddCategoryIfMissing(string categoryName)
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

        private RatingData AddRatingIfMissing(string ratingName)
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
            if (!this.games.TryGetValue(gameTitleUrlized, out var gameData))
            {
                throw new ArgumentException($"No game found for title {gameTitle}");
            }

            return gameData;
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
            if (!this.platforms.TryGetValue(platformAbbreviationUrlized, out var platformData))
            {
                throw new ArgumentException($"No platform found for abbreviation {platformAbbreviation}");
            }

            return platformData;
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
            if (!this.rawDataFiles.TryGetValue(filePath, out var rawDataFile))
            {
                throw new ArgumentException($"No raw data found for file path {filePath}");
            }

            return rawDataFile;
        }

        public List<StaticFileData> GetStaticFiles()
        {
            return this.staticFiles;
        }

        public TagData GetTag(string tagName)
        {
            var tagNameUrlized = new UrlizedString(tagName);
            if (!this.tags.TryGetValue(tagNameUrlized, out var tagData))
            {
                throw new ArgumentException($"No tag found for name {tagName}");
            }

            return tagData;
        }

        public List<TagData> GetTags()
        {
            return this.tags.Values.ToList();
        }

        private static void CreateOrMergeMultiKeyReferenceableData<T>(Dictionary<UrlizedString, T> index, string dataKey)
            where T : GlogDataFromIgdbGameMetadata, IGlogMultiKeyReferenceable
        {
            var dataKeyUrlized = new UrlizedString(dataKey);
            if (index.TryGetValue(dataKeyUrlized, out var existingData))
            {
                existingData.MergeReferenceableKey(dataKey);
            }
            else
            {
                var createdData = Activator.CreateInstance(typeof(T), new object[] { dataKey} ) as T;
                index.Add(dataKeyUrlized, createdData);
            }
        }

        public void LoadContent(IIgdbCache igdbCache)
        {
            // Reset the current index, while tracking some "old" data to detect update conflicts.
            this.pages.Clear();
            this.posts.Clear();
            this.rawDataFiles.Clear();
            this.staticFiles.Clear();

            var oldCategories = this.categories;
            this.categories = new Dictionary<UrlizedString, CategoryData>();

            var oldGames = this.games;
            this.games = new Dictionary<UrlizedString, GameData>();

            var oldPlatforms = this.platforms;
            this.platforms = new Dictionary<UrlizedString, PlatformData>();

            var oldRatings = this.ratings;
            this.ratings = new Dictionary<UrlizedString, RatingData>();

            var oldTags = this.tags;
            this.tags = new Dictionary<UrlizedString, TagData>();

            // Load game data from the IGDB cache.
            foreach (var igdbGame in igdbCache.GetAllGames())
            {
                var gameData = GameData.FromIgdbGame(igdbCache, igdbGame);

                var gameTitleUrlized = new UrlizedString(gameData.Title);
                this.games.Add(gameTitleUrlized, gameData);
            }

            // And platform data!
            foreach (var igdbPlatform in igdbCache.GetAllPlatforms())
            {
                var platformData = PlatformData.FromIgdbPlatform(igdbCache, igdbPlatform);

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

                var tagData = new TagData(tagName);
                this.tags.Add(new UrlizedString(tagName), tagData);
            }

            foreach (var igdbGameMetadata in igdbCache.GetAllGameMetadata())
            {
                var tagName = igdbGameMetadata.GetReferenceableValue();

                CreateOrMergeMultiKeyReferenceableData(this.tags, tagName);
            }

            if (!string.IsNullOrEmpty(this.inputFilesBasePath))
            {
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

                var mdPipeline = this.siteBuilder.GetMarkdownPipeline();

                var allPosts = new List<PostData>();
                foreach (var postPath in postPaths)
                {
                    try
                    {
                        var postData = PostData.MarkdownFromFilePath(mdPipeline, postPath);

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

                        var postGameTags = new Dictionary<UrlizedString, TagData>();
                        foreach (var game in postData.Games)
                        {
                            var gameData = this.GetGame(game);
                            gameData.LinkedPosts.Add(postData);

                            foreach (var tag in gameData.Tags)
                            {
                                var tagNameUrlized = new UrlizedString(tag);
                                postGameTags[tagNameUrlized] = this.GetTag(tag);
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
                    var pageData = PageData.MarkdownFromFilePath(mdPipeline, pageFilePath);
                    this.pages.Add(pageData);
                }
            }

            // Detect conflicts between old and updated data.
            this.CheckUpdatedReferenceableDataForConflict(oldCategories, this.categories);
            this.CheckUpdatedReferenceableDataForConflict(oldGames, this.games);
            this.CheckUpdatedReferenceableDataForConflict(oldPlatforms, this.platforms);
            this.CheckUpdatedReferenceableDataForConflict(oldRatings, this.ratings);
            this.CheckUpdatedReferenceableDataForConflict(oldTags, this.tags);
        }

        private void CheckUpdatedReferenceableDataForConflict<T>(Dictionary<UrlizedString, T> oldData, Dictionary<UrlizedString, T> newData)
            where T : IGlogReferenceable
        {
            foreach (var oldPair in oldData)
            {
                var oldKey = oldPair.Key;
                var oldDataId = oldPair.Value.GetDataId();

                if (!newData.TryGetValue(oldKey, out var newValue))
                {
                    // Is the data still around, but under a different key?
                    var newDataWithId = newData.Values.Where(v => v.GetDataId().Equals(oldDataId, StringComparison.Ordinal)).FirstOrDefault();

                    if (newDataWithId != null)
                    {
                        this.logger.LogError("Updated data index has a different key for {DataType} with data ID {DataId}: old key {OldKey} new key {NewKey}",
                            typeof(T).Name,
                            oldDataId,
                            oldKey,
                            newDataWithId.GetReferenceableKey());
                    }
                    else
                    {
                        this.logger.LogError("Updated data index is missing old {DataType} with data ID {DataId} and key {OldKey}",
                            typeof(T).Name,
                            oldDataId,
                            oldKey);
                    }
                }
                else
                {
                    var newDataId = newValue.GetDataId();
                    if (!newDataId.Equals(oldDataId, StringComparison.Ordinal))
                    {
                        this.logger.LogWarning("Updated data index has a different data ID for {DataType} with key {DataKey}: old data ID {OldDataId} new data ID {NewDataId}",
                            typeof(T).Name,
                            oldKey,
                            oldDataId,
                            newDataId);
                    }
                }
            }
        }
    }
}
