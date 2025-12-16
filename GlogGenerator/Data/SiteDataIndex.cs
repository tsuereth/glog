using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.IgdbApi;
using GlogGenerator.NewtonsoftJsonHelpers;
using GlogGenerator.RenderState;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GlogGenerator.Data
{
    public class SiteDataIndex : ISiteDataIndex
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,

            ContractResolver = new AlphaOrderedContractResolver(),
        };

        private readonly ILogger logger;
        private readonly string inputFilesBasePath;

        private List<string> nonContentGameNames = new List<string>();

        private List<SiteDataReference<CategoryData>> categoryReferences = new List<SiteDataReference<CategoryData>>();
        private List<SiteDataReference<GameData>> gameReferences = new List<SiteDataReference<GameData>>();
        private List<SiteDataReference<PlatformData>> platformReferences = new List<SiteDataReference<PlatformData>>();
        private List<SiteDataReference<RatingData>> ratingReferences = new List<SiteDataReference<RatingData>>();
        private List<SiteDataReference<TagData>> tagReferences = new List<SiteDataReference<TagData>>();

        private SiteDataLookups Lookups = new SiteDataLookups();

        private Dictionary<string, PageData> pages = new Dictionary<string, PageData>();
        private Dictionary<string, PostData> posts = new Dictionary<string, PostData>();
        private Dictionary<string, string> rawDataFiles = new Dictionary<string, string>();
        private List<StaticFileData> staticFiles = new List<StaticFileData>();

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

        public T GetDataWithOldLookup<T>(SiteDataReference<T> dataReference, GlogReferenceableLookup<T> oldDataLookup)
            where T : class, IGlogReferenceable
        {
            var dataLookup = this.Lookups.GetLookup<T>();

            var dataType = typeof(T);
            T data;
            if (!dataReference.GetIsResolved())
            {
                var referenceKey = dataReference.GetUnresolvedReferenceKey();

                // Do not check `oldDataLookup` here!
                // Unresolved references should never get tied to "old" data.

                if (!dataLookup.TryGetDataByReferenceableKey(referenceKey, out data))
                {
                    throw new ArgumentException($"No {dataType.Name} found for reference key {referenceKey}");
                }

                dataReference.SetData(data);
            }
            else
            {
                var referenceId = dataReference.GetResolvedReferenceId();

                if (!dataLookup.TryGetDataById(referenceId, out data))
                {
                    if (oldDataLookup == null)
                    {
                        throw new ArgumentException($"Reference ID {referenceId} is marked as resolved, but found no match in the current data index");
                    }
                    else
                    {
                        if (oldDataLookup.TryGetDataById(referenceId, out data))
                        {
                            // Well, if the current lookup didn't have this reference, but the "old" one does,
                            // then it'll need to get re-added to the current index.
                            dataLookup.AddData(data);
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
            return this.Lookups.GetValues<CategoryData>();
        }

        public bool HasGame(string gameTitle)
        {
            return this.Lookups.HasDataByReferenceableKey<GameData>(gameTitle);
        }

        public GameData GetGame(string gameTitle)
        {
            return this.Lookups.GetDataByReferenceableKey<GameData>(gameTitle);
        }

        public List<GameData> GetGames()
        {
            return this.Lookups.GetValues<GameData>();
        }

        public List<PageData> GetPages()
        {
            return this.pages.Values.ToList();
        }

        public PlatformData GetPlatform(string platformAbbreviation)
        {
            return this.Lookups.GetDataByReferenceableKey<PlatformData>(platformAbbreviation);
        }

        public List<PlatformData> GetPlatforms()
        {
            return this.Lookups.GetValues<PlatformData>();
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
            return this.Lookups.GetValues<RatingData>();
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
            return this.Lookups.GetDataByReferenceableKey<TagData>(tagName);
        }

        public List<TagData> GetTags()
        {
            return this.Lookups.GetValues<TagData>();
        }

        public void SetNonContentGameNames(List<string> gameNames)
        {
            this.nonContentGameNames = gameNames;
        }

        private void UpdateReferencesFromContent(SiteDataLookups oldLookups, ContentParser contentParser, bool includeDrafts)
        {
            this.rawDataFiles.Clear();
            this.staticFiles.Clear();

            var oldPages = this.pages;
            this.pages = new Dictionary<string, PageData>();

            var oldPosts = this.posts;
            this.posts = new Dictionary<string, PostData>();

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
                        var postId = PostData.PostIdFromFilePath(contentParser, postPath);
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
                            postData = PostData.MarkdownFromFilePath(contentParser, postPath, this);
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
                                categoryData = this.GetDataWithOldLookup(categoryReference, oldLookups.Categories);
                            }
                            catch (ArgumentException)
                            {
                                categoryData = new CategoryData()
                                {
                                    Name = categoryReference.GetUnresolvedReferenceKey(),
                                };

                                this.Lookups.AddData<CategoryData>(categoryData);
                            }

                            categoryData.LinkedPostIds.Add(postId);
                        }

                        var postGameTags = new Dictionary<string, TagData>();
                        if (postData.Games != null)
                        {
                            foreach (var gameReference in postData.Games)
                            {
                                var gameData = this.GetDataWithOldLookup(gameReference, oldLookups.Games);
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
                                var platformData = this.GetDataWithOldLookup(platformReference, oldLookups.Platforms);
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
                                    ratingData = this.GetDataWithOldLookup(ratingReference, oldLookups.Ratings);
                                }
                                catch (ArgumentException)
                                {
                                    ratingData = new RatingData()
                                    {
                                        Name = ratingReference.GetUnresolvedReferenceKey(),
                                    };

                                    this.Lookups.AddData<RatingData>(ratingData);
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
                    var pageId = PageData.PageIdFromFilePath(contentParser, pageFilePath);
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
                        pageData = PageData.MarkdownFromFilePath(contentParser, pageFilePath);
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
            this.CheckUpdatedReferenceableDataForConflict(oldLookups.GetValues<CategoryData>(), this.Lookups.GetValues<CategoryData>());
            this.CheckUpdatedReferenceableDataForConflict(oldLookups.GetValues<GameData>(), this.Lookups.GetValues<GameData>());
            this.CheckUpdatedReferenceableDataForConflict(oldLookups.GetValues<PlatformData>(), this.Lookups.GetValues<PlatformData>());
            this.CheckUpdatedReferenceableDataForConflict(oldLookups.GetValues<RatingData>(), this.Lookups.GetValues<RatingData>());
            this.CheckUpdatedReferenceableDataForConflict(oldLookups.GetValues<TagData>(), this.Lookups.GetValues<TagData>());
        }

        public void LoadContent(IIgdbCache igdbCache, ContentParser contentParser, bool includeDrafts)
        {
            // Reset the current index, while tracking some* old data to detect update conflicts.
            // (*) Only retain data associated with DataReferences which "should update" on changed data;
            // other data (whose references should not be updated) should instead be thrown away.

            var updateCategoryReferenceIds = this.categoryReferences.Where(r => r.GetShouldUpdateOnDataChange()).Select(r => r.GetResolvedReferenceId());
            var oldCategories = new GlogReferenceableLookup<CategoryData>(this.Lookups.Categories.GetValues().Where(d => updateCategoryReferenceIds.Contains(d.GetDataId())));
            this.categoryReferences.RemoveAll(r => !r.GetShouldUpdateOnDataChange());

            var updateGameReferenceIds = this.gameReferences.Where(r => r.GetShouldUpdateOnDataChange()).Select(r => r.GetResolvedReferenceId());
            var oldGames = new GlogReferenceableLookup<GameData>(this.Lookups.Games.GetValues().Where(d => updateGameReferenceIds.Contains(d.GetDataId())));
            this.gameReferences.RemoveAll(r => !r.GetShouldUpdateOnDataChange());

            var updatePlatformReferenceIds = this.platformReferences.Where(r => r.GetShouldUpdateOnDataChange()).Select(r => r.GetResolvedReferenceId());
            var oldPlatforms = new GlogReferenceableLookup<PlatformData>(this.Lookups.Platforms.GetValues().Where(d => updatePlatformReferenceIds.Contains(d.GetDataId())));
            this.platformReferences.RemoveAll(r => !r.GetShouldUpdateOnDataChange());

            var updateRatingReferenceIds = this.ratingReferences.Where(r => r.GetShouldUpdateOnDataChange()).Select(r => r.GetResolvedReferenceId());
            var oldRatings = new GlogReferenceableLookup<RatingData>(this.Lookups.Ratings.GetValues().Where(d => updateRatingReferenceIds.Contains(d.GetDataId())));
            this.ratingReferences.RemoveAll(r => !r.GetShouldUpdateOnDataChange());

            var updateTagReferenceIds = this.tagReferences.Where(r => r.GetShouldUpdateOnDataChange()).Select(r => r.GetResolvedReferenceId());
            var oldTags = new GlogReferenceableLookup<TagData>(this.Lookups.Tags.GetValues().Where(d => updateTagReferenceIds.Contains(d.GetDataId())));
            this.tagReferences.RemoveAll(r => !r.GetShouldUpdateOnDataChange());

            this.Lookups.ClearAll();

            // Load game data from the IGDB cache.
            foreach (var igdbGame in igdbCache.GetAllGames())
            {
                var gameEntityReference = new IgdbGameReference(igdbGame, igdbCache);
                var gameData = GameData.FromIgdbGameReference(gameEntityReference);
                gameData.PopulateRelatedIgdbData(igdbCache);

                this.Lookups.AddData<GameData>(gameData);
            }

            // And platform data!
            foreach (var igdbPlatform in igdbCache.GetAllPlatforms())
            {
                var platformEntityReference = new IgdbPlatformReference(igdbPlatform);
                var platformData = PlatformData.FromIgdbPlatformReference(platformEntityReference);
                platformData.PopulateRelatedIgdbData(igdbCache);

                this.Lookups.AddData<PlatformData>(platformData);
            }

            // Prepare tags from game metadata.
            foreach (var igdbGameMetadata in igdbCache.GetAllGameMetadata())
            {
                var metadataReference = new IgdbMetadataReference(igdbGameMetadata);
                var tagData = new TagData(metadataReference);
                if (this.Lookups.TryGetDataById<TagData>(tagData.GetDataId(), out var existingTag))
                {
                    existingTag.MergeIgdbMetadataReference(metadataReference);
                }
                else
                {
                    this.Lookups.AddData<TagData>(tagData);
                }
            }

            var oldLookups = new SiteDataLookups()
            {
                Categories = oldCategories,
                Games = oldGames,
                Platforms = oldPlatforms,
                Ratings = oldRatings,
                Tags = oldTags,
            };

            this.UpdateReferencesFromContent(oldLookups, contentParser, includeDrafts);
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
                var game = this.Lookups.GetDataById<GameData>(gameDataId);

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
            var unreferencedCategoryIds = this.Lookups.GetIds<CategoryData>().Where(id => !referencedCategoryIds.Contains(id));
            foreach (var unreferencedCategoryId in unreferencedCategoryIds)
            {
                this.Lookups.RemoveDataById<CategoryData>(unreferencedCategoryId);
            }

            var referencedGameIds = this.gameReferences.Select(r => r.GetResolvedReferenceId()).Distinct();
            var unreferencedGameIds = this.Lookups.GetIds<GameData>().Where(id => !referencedGameIds.Contains(id));
            foreach (var unreferencedGameId in unreferencedGameIds)
            {
                if (igdbCache != null)
                {
                    var igdbGame = this.Lookups.GetDataById<GameData>(unreferencedGameId).GetIgdbEntityReference();
                    igdbCache.RemoveEntityById(typeof(IgdbGame), igdbGame.IgdbEntityId.Value);
                }

                this.Lookups.RemoveDataById<GameData>(unreferencedGameId);
            }

            var referencedPlatformIds = this.platformReferences.Select(r => r.GetResolvedReferenceId()).Distinct();
            var unreferencedPlatformIds = this.Lookups.GetIds<PlatformData>().Where(id => !referencedPlatformIds.Contains(id));
            foreach (var unreferencedPlatformId in unreferencedPlatformIds)
            {
                if (igdbCache != null)
                {
                    var igdbPlatform = this.Lookups.GetDataById<PlatformData>(unreferencedPlatformId).GetIgdbEntityReference();
                    igdbCache.RemoveEntityById(typeof(IgdbPlatform), igdbPlatform.IgdbEntityId.Value);
                }

                this.Lookups.RemoveDataById<PlatformData>(unreferencedPlatformId);
            }

            var referencedRatingIds = this.ratingReferences.Select(r => r.GetResolvedReferenceId()).Distinct();
            var unreferencedRatingIds = this.Lookups.GetIds<RatingData>().Where(id => !referencedRatingIds.Contains(id));
            foreach (var unreferencedRatingId in unreferencedRatingIds)
            {
                this.Lookups.RemoveDataById<RatingData>(unreferencedRatingId);
            }

            var referencedTagIds = this.tagReferences.Select(r => r.GetResolvedReferenceId()).Distinct();
            var unreferencedTagIds = this.Lookups.GetIds<TagData>().Where(id => !referencedTagIds.Contains(id));
            foreach (var unreferencedTagId in unreferencedTagIds)
            {
                if (igdbCache != null)
                {
                    var igdbEntities = this.Lookups.GetDataById<TagData>(unreferencedTagId).GetIgdbEntityReferences();
                    foreach (var igdbEntity in igdbEntities)
                    {
                        igdbCache.RemoveEntityById(igdbEntity.IgdbEntityType, igdbEntity.IgdbEntityId.Value);
                    }
                }

                this.Lookups.RemoveDataById<TagData>(unreferencedTagId);
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

        public void RewriteSourceContent(ContentParser contentParser)
        {
            foreach (var page in this.pages.Values)
            {
                page.RewriteSourceFile(contentParser);
            }

            foreach (var post in this.posts.Values)
            {
                post.RewriteSourceFile(contentParser, this);
            }
        }
        private static string JsonFilePathForReferenceType(string directoryPath, string typeName)
        {
            return Path.Combine(directoryPath, $"index_{typeName}.json");
        }

        private static void WriteDataItemsToJsonFile<T>(IEnumerable<T> dataItems, string directoryPath, string typeName)
            where T : class, IGlogReferenceable
        {
            var records = new SortedDictionary<string, object>();
            foreach (var dataItem in dataItems)
            {
                records.Add(dataItem.GetReferenceableKey(), dataItem.GetReferenceProperties());
            }

            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);
            var jsonFilePath = JsonFilePathForReferenceType(directoryPath, typeName);
            using (var fileStream = new FileStream(jsonFilePath, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream) { NewLine = "\n" })
            using (var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented })
            {
                jsonSerializer.Serialize(jsonWriter, records);
                streamWriter.Flush();
            }
        }

        public void WriteToJsonFiles(string directoryPath)
        {
            // Category references are trivial, don't bother writing them down.
            WriteDataItemsToJsonFile(this.Lookups.GetValues<GameData>(), directoryPath, "games");
            WriteDataItemsToJsonFile(this.Lookups.GetValues<PlatformData>(), directoryPath, "platforms");
            // Ratings references are trivial, don't bother writing them down.
            WriteDataItemsToJsonFile(this.Lookups.GetValues<TagData>(), directoryPath, "tags");
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
