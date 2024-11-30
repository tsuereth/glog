using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.IgdbApi;
using GlogGenerator.RenderState;
using Microsoft.Extensions.Logging;

namespace GlogGenerator.Data
{
    public class SiteDataIndex : ISiteDataIndex
    {
        private readonly ILogger logger;
        private readonly string inputFilesBasePath;

        private List<string> nonContentGameNames = new List<string>();

        private List<SiteDataReference<CategoryData>> categoryReferences = new List<SiteDataReference<CategoryData>>();
        private List<SiteDataReference<GameData>> gameReferences = new List<SiteDataReference<GameData>>();
        private List<SiteDataReference<PlatformData>> platformReferences = new List<SiteDataReference<PlatformData>>();
        private List<SiteDataReference<RatingData>> ratingReferences = new List<SiteDataReference<RatingData>>();
        private List<SiteDataReference<TagData>> tagReferences = new List<SiteDataReference<TagData>>();

        private Dictionary<string, CategoryData> categories = new Dictionary<string, CategoryData>();
        private Dictionary<string, GameData> games = new Dictionary<string, GameData>();
        private Dictionary<string, PageData> pages = new Dictionary<string, PageData>();
        private Dictionary<string, PlatformData> platforms = new Dictionary<string, PlatformData>();
        private Dictionary<string, PostData> posts = new Dictionary<string, PostData>();
        private Dictionary<string, RatingData> ratings = new Dictionary<string, RatingData>();
        private Dictionary<string, string> rawDataFiles = new Dictionary<string, string>();
        private List<StaticFileData> staticFiles = new List<StaticFileData>();
        private Dictionary<string, TagData> tags = new Dictionary<string, TagData>();

        private Dictionary<string, string> gameDataIdsByName = new Dictionary<string, string>();
        private Dictionary<string, string> tagDataIdsByNameUrlized = new Dictionary<string, string>();

        public SiteDataIndex(
            ILogger logger,
            string inputFilesBasePath)
        {
            this.logger = logger;
            this.inputFilesBasePath = inputFilesBasePath;
        }

        public SiteDataReference<T> CreateReference<T>(string referenceKey, bool shouldUpdateOnDataChange)
            where T : IGlogReferenceable
        {
            var dataReference = new SiteDataReference<T>(referenceKey, shouldUpdateOnDataChange);

            var dataType = typeof(T);
            if (dataType == typeof(CategoryData))
            {
                this.categoryReferences.Add(dataReference as SiteDataReference<CategoryData>);
            }
            else if (dataType == typeof(GameData))
            {
                this.gameReferences.Add(dataReference as SiteDataReference<GameData>);
            }
            else if (dataType == typeof(PlatformData))
            {
                this.platformReferences.Add(dataReference as SiteDataReference<PlatformData>);
            }
            else if (dataType == typeof(RatingData))
            {
                this.ratingReferences.Add(dataReference as SiteDataReference<RatingData>);
            }
            else if (dataType == typeof(TagData))
            {
                this.tagReferences.Add(dataReference as SiteDataReference<TagData>);
            }
            else
            {
                throw new NotImplementedException();
            }

            return dataReference;
        }

        public T GetData<T>(SiteDataReference<T> dataReference)
            where T : class, IGlogReferenceable
        {
            return this.GetDataWithOldLookup<T>(dataReference, null);
        }

        public T GetDataWithOldLookup<T>(SiteDataReference<T> dataReference, Dictionary<string, T> oldDataLookup)
            where T : class, IGlogReferenceable
        {
            var dataType = typeof(T);
            T data;

            Dictionary<string, T> dataLookup;
            if (dataType == typeof(CategoryData))
            {
                dataLookup = this.categories as Dictionary<string, T>;
            }
            else if (dataType == typeof(GameData))
            {
                dataLookup = this.games as Dictionary<string, T>;
            }
            else if (dataType == typeof(PlatformData))
            {
                dataLookup = this.platforms as Dictionary<string, T>;
            }
            else if (dataType == typeof(RatingData))
            {
                dataLookup = this.ratings as Dictionary<string, T>;
            }
            else if (dataType == typeof(TagData))
            {
                dataLookup = this.tags as Dictionary<string, T>;
            }
            else
            {
                throw new NotImplementedException($"Missing lookup for type {dataType.Name}");
            }

            if (!dataReference.GetIsResolved())
            {
                var referenceKey = dataReference.GetUnresolvedReferenceKey();

                // Do not check `oldDataLookup` here!
                // Unresolved references should never get tied to "old" data.

                data = dataLookup.Values.Where(v => v.MatchesReferenceableKey(referenceKey)).FirstOrDefault();
                if (data == null)
                {
                    throw new ArgumentException($"No {dataType.Name} found for reference key {referenceKey}");
                }

                dataReference.SetData(data);
            }
            else
            {
                var referenceId = dataReference.GetResolvedReferenceId();

                if (dataLookup.ContainsKey(referenceId))
                {
                    data = dataLookup[referenceId];
                }
                else
                {
                    if (oldDataLookup == null)
                    {
                        throw new ArgumentException($"Reference ID {referenceId} is marked as resolved, but found no match in the current data index");
                    }
                    else
                    {
                        if (oldDataLookup.ContainsKey(referenceId))
                        {
                            data = oldDataLookup[referenceId];

                            // Well, if the current lookup didn't have this reference, but the "old" one does,
                            // then it'll need to get re-added to the current index.
                            dataLookup[referenceId] = data;
                        }
                        else
                        {
                            throw new ArgumentException($"Reference ID {referenceId} is marked as resolved, but found no match in the current OR old data index");
                        }
                    }
                }
            }

            return data;
        }

        public List<CategoryData> GetCategories()
        {
            return this.categories.Values.ToList();
        }

        public bool HasGame(string gameTitle)
        {
            return this.gameDataIdsByName.ContainsKey(gameTitle);
        }

        public GameData GetGame(string gameTitle)
        {
            if (!this.gameDataIdsByName.TryGetValue(gameTitle, out var gameDataId))
            {
                throw new ArgumentException($"No game found for title {gameTitle}");
            }

            return this.games[gameDataId];
        }

        public List<GameData> GetGames()
        {
            return this.games.Values.ToList();
        }

        public List<PageData> GetPages()
        {
            return this.pages.Values.ToList();
        }

        public PlatformData GetPlatform(string platformAbbreviation)
        {
            var platform = this.platforms.Values.Where(d => d.Abbreviation.Equals(platformAbbreviation, StringComparison.Ordinal)).FirstOrDefault();
            if (platform == null)
            {
                throw new ArgumentException($"No platform found for abbreviation {platformAbbreviation}");
            }

            return platform;
        }

        public List<PlatformData> GetPlatforms()
        {
            return this.platforms.Values.ToList();
        }

        public PostData GetPostById(string postId)
        {
            return this.posts[postId];
        }

        public List<PostData> GetPosts()
        {
            return this.posts.Values.OrderByDescending(p => p.Date).ToList();
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
            var tagNameUrlized = UrlizedString.Urlize(tagName);
            if (!this.tagDataIdsByNameUrlized.TryGetValue(tagNameUrlized, out var tagDataId))
            {
                throw new ArgumentException($"No tag found for name {tagName}");
            }

            return this.tags[tagDataId];
        }

        public List<TagData> GetTags()
        {
            return this.tags.Values.ToList();
        }

        public void SetNonContentGameNames(List<string> gameNames)
        {
            this.nonContentGameNames = gameNames;
        }

        public void LoadContent(IIgdbCache igdbCache, Markdig.MarkdownPipeline markdownPipeline, bool includeDrafts)
        {
            // Reset the current index, while tracking some* old data to detect update conflicts.
            // (*) Only retain data associated with DataReferences which "should update" on changed data;
            // other data (whose references should not be updated) should instead be thrown away.
            this.rawDataFiles.Clear();
            this.staticFiles.Clear();

            var oldPages = this.pages;
            this.pages = new Dictionary<string, PageData>();

            var oldPosts = this.posts;
            this.posts = new Dictionary<string, PostData>();

            var updateCategoryReferenceIds = this.categoryReferences.Where(r => r.GetShouldUpdateOnDataChange()).Select(r => r.GetResolvedReferenceId());
            var oldCategories = this.categories.Where(kv => updateCategoryReferenceIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            this.categoryReferences.RemoveAll(r => !r.GetShouldUpdateOnDataChange());
            this.categories = new Dictionary<string, CategoryData>();

            var updateGameReferenceIds = this.gameReferences.Where(r => r.GetShouldUpdateOnDataChange()).Select(r => r.GetResolvedReferenceId());
            var oldGames = this.games.Where(kv => updateGameReferenceIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            this.gameReferences.RemoveAll(r => !r.GetShouldUpdateOnDataChange());
            this.games = new Dictionary<string, GameData>();
            this.gameDataIdsByName.Clear();

            var updatePlatformReferenceIds = this.platformReferences.Where(r => r.GetShouldUpdateOnDataChange()).Select(r => r.GetResolvedReferenceId());
            var oldPlatforms = this.platforms.Where(kv => updatePlatformReferenceIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            this.platformReferences.RemoveAll(r => !r.GetShouldUpdateOnDataChange());
            this.platforms = new Dictionary<string, PlatformData>();

            var updateRatingReferenceIds = this.ratingReferences.Where(r => r.GetShouldUpdateOnDataChange()).Select(r => r.GetResolvedReferenceId());
            var oldRatings = this.ratings.Where(kv => updateRatingReferenceIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            this.ratingReferences.RemoveAll(r => !r.GetShouldUpdateOnDataChange());
            this.ratings = new Dictionary<string, RatingData>();

            var updateTagReferenceIds = this.tagReferences.Where(r => r.GetShouldUpdateOnDataChange()).Select(r => r.GetResolvedReferenceId());
            var oldTags = this.tags.Where(kv => updateTagReferenceIds.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            this.tagReferences.RemoveAll(r => !r.GetShouldUpdateOnDataChange());
            this.tags = new Dictionary<string, TagData>();
            this.tagDataIdsByNameUrlized.Clear();

            // Load game data from the IGDB cache.
            foreach (var igdbGame in igdbCache.GetAllGames())
            {
                var gameData = GameData.FromIgdbGame(igdbCache, igdbGame);

                this.games.Add(gameData.GetDataId(), gameData);

                var gameName = gameData.Title;
                this.gameDataIdsByName[gameName] = gameData.GetDataId();
            }

            // And platform data!
            foreach (var igdbPlatform in igdbCache.GetAllPlatforms())
            {
                var platformData = PlatformData.FromIgdbPlatform(igdbCache, igdbPlatform);

                this.platforms.Add(platformData.GetDataId(), platformData);
            }

            // Prepare tags from game metadata.
            foreach (var igdbGameCategory in (IgdbGameCategory[])Enum.GetValues(typeof(IgdbGameCategory)))
            {
                if (igdbGameCategory == IgdbGameCategory.None)
                {
                    continue;
                }

                var tagData = new TagData(typeof(IgdbGameCategory), igdbGameCategory.Description());

                this.tags.Add(tagData.GetDataId(), tagData);

                var tagNameUrlized = UrlizedString.Urlize(tagData.Name);
                this.tagDataIdsByNameUrlized[tagNameUrlized] = tagData.GetDataId();
            }

            foreach (var igdbGameMetadata in igdbCache.GetAllGameMetadata())
            {
                var tagType = igdbGameMetadata.GetType();
                var tagName = igdbGameMetadata.GetReferenceString(igdbCache);

                var tagNameUrlized = UrlizedString.Urlize(tagName);
                if (this.tagDataIdsByNameUrlized.TryGetValue(tagNameUrlized, out var tagDataId))
                {
                    var tagDataMatchingKey = this.tags[tagDataId];
                    tagDataMatchingKey.MergeReferenceableKey(tagType, tagName);
                }
                else
                {
                    var createdTag = new TagData(tagType, tagName);
                    this.tags.Add(createdTag.GetDataId(), createdTag);
                    this.tagDataIdsByNameUrlized[tagNameUrlized] = createdTag.GetDataId();
                }
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

                foreach (var postPath in postPaths)
                {
                    try
                    {
                        var postId = PostData.PostIdFromFilePath(markdownPipeline, postPath);
                        PostData postData;

                        // Was this post loaded before?
                        // A previously-loaded PostData, with previously-processed references, may be more up-to-date
                        // than attempting to re-parse the post from its original source file!
                        if (oldPosts.ContainsKey(postId))
                        {
                            // TODO?: Compare the old post's file mtime to the mtime of the local file?
                            postData = oldPosts[postId];
                        }
                        else
                        {
                            postData = PostData.MarkdownFromFilePath(markdownPipeline, postPath, this);
                        }

                        if (!includeDrafts && postData.Draft == true)
                        {
                            continue;
                        }

                        this.posts[postId] = postData;

                        foreach (var categoryReference in postData.Categories)
                        {
                            CategoryData categoryData;
                            try
                            {
                                categoryData = this.GetDataWithOldLookup(categoryReference, oldCategories);
                            }
                            catch (ArgumentException)
                            {
                                categoryData = new CategoryData()
                                {
                                    Name = categoryReference.GetUnresolvedReferenceKey(),
                                };

                                this.categories[categoryData.GetDataId()] = categoryData;
                            }

                            categoryData.LinkedPostIds.Add(postId);
                        }

                        var postGameTags = new Dictionary<string, TagData>();
                        if (postData.Games != null)
                        {
                            foreach (var gameReference in postData.Games)
                            {
                                var gameData = this.GetDataWithOldLookup(gameReference, oldGames);
                                gameData.LinkedPostIds.Add(postId);

                                foreach (var tagName in gameData.Tags)
                                {
                                    var tagData = this.GetTag(tagName);
                                    postGameTags[tagData.GetDataId()] = tagData;
                                }
                            }
                        }

                        foreach (var tagData in postGameTags.Values)
                        {
                            tagData.LinkedPostIds.Add(postId);
                        }

                        if (postData.Platforms != null)
                        {
                            foreach (var platformReference in postData.Platforms)
                            {
                                var platformData = this.GetDataWithOldLookup(platformReference, oldPlatforms);
                                platformData.LinkedPostIds.Add(postId);
                            }
                        }

                        if (postData.Ratings != null)
                        {
                            foreach (var ratingReference in postData.Ratings)
                            {
                                RatingData ratingData;
                                try
                                {
                                    ratingData = this.GetDataWithOldLookup(ratingReference, oldRatings);
                                }
                                catch (ArgumentException)
                                {
                                    ratingData = new RatingData()
                                    {
                                        Name = ratingReference.GetUnresolvedReferenceKey(),
                                    };

                                    this.ratings[ratingData.GetDataId()] = ratingData;
                                }

                                ratingData.LinkedPostIds.Add(postId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidDataException($"Failed to load post from {postPath}", ex);
                    }
                }

                var additionalPageFilePaths = new List<string>()
                {
                    Path.Combine(this.inputFilesBasePath, "content", "backlog.md"),
                    Path.Combine(this.inputFilesBasePath, "content", "upcoming.md"),
                };
                foreach (var pageFilePath in additionalPageFilePaths)
                {
                    var pageId = PageData.PageIdFromFilePath(markdownPipeline, pageFilePath);
                    PageData pageData;

                    // Was this page loaded before?
                    // A previously-loaded PageData, with previously-processed references, may be more up-to-date
                    // than attempting to re-parse the page from its original source file!
                    if (oldPages.ContainsKey(pageId))
                    {
                        // TODO?: Compare the old page's file mtime to the mtime of the local file?
                        pageData = oldPages[pageId];
                    }
                    else
                    {
                        pageData = PageData.MarkdownFromFilePath(markdownPipeline, pageFilePath);
                    }

                    this.pages[pageId] = pageData;
                }
            }

            // Some games are referenced by configuration, outside of content posts and pages.
            // Ensure that SOME tracked reference exists for those games.
            foreach (var gameName in this.nonContentGameNames)
            {
                this.CreateReference<GameData>(gameName, false);
            }

            // Detect conflicts between old and updated data.
            this.CheckUpdatedReferenceableDataForConflict(oldCategories.Values, this.categories.Values);
            this.CheckUpdatedReferenceableDataForConflict(oldGames.Values, this.games.Values);
            this.CheckUpdatedReferenceableDataForConflict(oldPlatforms.Values, this.platforms.Values);
            this.CheckUpdatedReferenceableDataForConflict(oldRatings.Values, this.ratings.Values);
            this.CheckUpdatedReferenceableDataForConflict(oldTags.Values, this.tags.Values);
        }

        public void ResolveReferences()
        {
            foreach (var dataReference in this.categoryReferences)
            {
                _ = this.GetData(dataReference);
            }

            foreach (var dataReference in this.gameReferences)
            {
                _ = this.GetData(dataReference);
            }

            foreach (var dataReference in this.platformReferences)
            {
                _ = this.GetData(dataReference);
            }

            foreach (var dataReference in this.ratingReferences)
            {
                _ = this.GetData(dataReference);
            }

            foreach (var dataReference in this.tagReferences)
            {
                _ = this.GetData(dataReference);
            }
        }

        public void RegisterAutomaticReferences(IIgdbCache igdbCache)
        {
            var allIgdbPlatforms = igdbCache.GetAllPlatforms();

            // For every currently-referenced game, register references to that game's associated tags.
            var referencedGameDataIds = this.gameReferences.Where(r => r.GetIsResolved()).Select(r => r.GetResolvedReferenceId()).Distinct();
            foreach (var gameDataId in referencedGameDataIds)
            {
                var game = this.games[gameDataId];

                foreach (var tag in game.Tags)
                {
                    var gameTagReference = this.CreateReference<TagData>(tag, false);
                    _ = this.GetData(gameTagReference); // Resolve the reference, to ensure it isn't deleted later.
                }

                // And... if the game's title is based on some platform data, force those platforms to persist in the cache.
                if (game.TitleIncludesPlatforms)
                {
                    foreach (var platform in game.TitlePlatforms)
                    {
                        var igdbPlatform = allIgdbPlatforms.Where(p => p.GetReferenceString(igdbCache).Equals(platform, StringComparison.Ordinal)).First();
                        igdbPlatform.SetForcePersistInCache();
                    }
                }
            }
        }

        public void RemoveUnreferencedData(IIgdbCache igdbCache)
        {
            var referencedCategoryIds = this.categoryReferences.Select(r => r.GetResolvedReferenceId()).Distinct();
            var unreferencedCategoryKeypairs = this.categories.Where(kv => !referencedCategoryIds.Contains(kv.Value.GetDataId()));
            foreach (var unreferencedCategoryKeypair in unreferencedCategoryKeypairs)
            {
                this.categories.Remove(unreferencedCategoryKeypair.Key);
            }

            var referencedGameIds = this.gameReferences.Select(r => r.GetResolvedReferenceId()).Distinct();
            var unreferencedGameKeypairs = this.games.Where(kv => !referencedGameIds.Contains(kv.Value.GetDataId()));
            foreach (var unreferencedGameKeypair in unreferencedGameKeypairs)
            {
                this.games.Remove(unreferencedGameKeypair.Key);

                var gameName = unreferencedGameKeypair.Value.Title;
                this.gameDataIdsByName.Remove(gameName);

                if (igdbCache != null)
                {
                    var gameDataId = unreferencedGameKeypair.Value.GetDataId();
                    igdbCache.RemoveEntityByUniqueIdString(typeof(IgdbGame), gameDataId);
                }
            }

            var referencedPlatformIds = this.platformReferences.Select(r => r.GetResolvedReferenceId()).Distinct();
            var unreferencedPlatformKeypairs = this.platforms.Where(kv => !referencedPlatformIds.Contains(kv.Value.GetDataId()));
            foreach (var unreferencedPlatformKeypair in unreferencedPlatformKeypairs)
            {
                this.platforms.Remove(unreferencedPlatformKeypair.Key);

                if (igdbCache != null)
                {
                    var platformDataId = unreferencedPlatformKeypair.Value.GetDataId();
                    igdbCache.RemoveEntityByUniqueIdString(typeof(IgdbPlatform), platformDataId);
                }
            }

            var referencedRatingIds = this.ratingReferences.Select(r => r.GetResolvedReferenceId()).Distinct();
            var unreferencedRatingKeypairs = this.ratings.Where(kv => !referencedRatingIds.Contains(kv.Value.GetDataId()));
            foreach (var unreferencedRatingKeypair in unreferencedRatingKeypairs)
            {
                this.ratings.Remove(unreferencedRatingKeypair.Key);
            }

            var referencedTagIds = this.tagReferences.Select(r => r.GetResolvedReferenceId()).Distinct();
            var unreferencedTagKeypairs = this.tags.Where(kv => !referencedTagIds.Contains(kv.Value.GetDataId()));
            foreach (var unreferencedTagKeypair in unreferencedTagKeypairs)
            {
                this.tags.Remove(unreferencedTagKeypair.Key);

                var tagNameUrlized = UrlizedString.Urlize(unreferencedTagKeypair.Value.Name);
                this.tagDataIdsByNameUrlized.Remove(tagNameUrlized);

                if (igdbCache != null)
                {
                    foreach (var typedKey in unreferencedTagKeypair.Value.GetReferenceableTypedKeys())
                    {
                        var tagDataType = typedKey.Item1;
                        var tagDataId = typedKey.Item2;

                        igdbCache.RemoveEntityByReferenceString(tagDataType, tagDataId);
                    }
                }
            }
        }

        public void LinkPostsToAssociatedGames()
        {
            foreach (var kv in this.posts)
            {
                var postId = kv.Key;
                var postData = kv.Value;

                if (postData.Games != null)
                {
                    foreach (var postGameReference in postData.Games)
                    {
                        var postGameData = this.GetData(postGameReference);

                        var parentGameTitles = postGameData.GetParentGames(this);
                        var otherReleaseTitles = postGameData.GetOtherReleases(this);

                        foreach (var parentGameTitle in parentGameTitles)
                        {
                            var parentGameData = this.GetGame(parentGameTitle);
                            parentGameData.LinkedPostIds.Add(postId);
                        }

                        foreach (var otherReleaseTitle in otherReleaseTitles)
                        {
                            var otherReleaseData = this.GetGame(otherReleaseTitle);
                            otherReleaseData.LinkedPostIds.Add(postId);
                        }
                    }
                }
            }
        }

        public void RewriteSourceContent(Markdig.MarkdownPipeline markdownPipeline)
        {
            foreach (var page in this.pages.Values)
            {
                page.RewriteSourceFile(markdownPipeline);
            }

            foreach (var post in this.posts.Values)
            {
                post.RewriteSourceFile(markdownPipeline, this);
            }
        }

        private void CheckUpdatedReferenceableDataForConflict<T>(IReadOnlyCollection<T> oldDataCollection, IReadOnlyCollection<T> newDataCollection)
            where T : IGlogReferenceable
        {
            foreach (var oldValue in oldDataCollection)
            {
                var oldKey = oldValue.GetReferenceableKey();
                var oldDataId = oldValue.GetDataId();

                var newDataWithKey = newDataCollection.Where(v => v.GetReferenceableKey().Equals(oldKey, StringComparison.Ordinal)).FirstOrDefault();
                if (newDataWithKey == null)
                {
                    // Is the data still around, but under a different key?
                    var newDataWithId = newDataCollection.Where(v => v.GetDataId().Equals(oldDataId, StringComparison.Ordinal)).FirstOrDefault();

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
                    var newDataId = newDataWithKey.GetDataId();
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
