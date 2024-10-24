﻿using System;
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

        public SiteDataIndex(
            ILogger logger,
            string inputFilesBasePath)
        {
            this.logger = logger;
            this.inputFilesBasePath = inputFilesBasePath;
        }

        public SiteDataReference<T> CreateReference<T>(string referenceKey)
            where T : IGlogReferenceable
        {
            var dataReference = new SiteDataReference<T>(referenceKey);

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

        public GameData GetGame(string gameTitle)
        {
            var game = this.games.Values.Where(d => d.Title.Equals(gameTitle, StringComparison.Ordinal)).FirstOrDefault();
            if (game == null)
            {
                throw new ArgumentException($"No game found for title {gameTitle}");
            }

            return game;
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
            var tag = this.tags.Values.Where(d => d.MatchesReferenceableKey(tagName)).FirstOrDefault();
            if (tag == null)
            {
                throw new ArgumentException($"No tag found for name {tagName}");
            }

            return tag;
        }

        public List<TagData> GetTags()
        {
            return this.tags.Values.ToList();
        }

        private static void CreateOrMergeMultiKeyReferenceableData<T>(Dictionary<string, T> index, Type dataType, string dataKey)
            where T : GlogDataFromIgdbGameMetadata, IGlogMultiKeyReferenceable
        {
            var dataMatchingKey = index.Values.Where(v => v.ShouldMergeWithReferenceableKey(dataKey)).FirstOrDefault();
            if (dataMatchingKey != null)
            {
                dataMatchingKey.MergeReferenceableKey(dataType, dataKey);
            }
            else
            {
                var createdData = Activator.CreateInstance(typeof(T), new object[] { dataType, dataKey } ) as T;
                index.Add(createdData.GetDataId(), createdData);
            }
        }

        public void SetNonContentGameNames(List<string> gameNames)
        {
            this.nonContentGameNames = gameNames;
        }

        public void LoadContent(IIgdbCache igdbCache, Markdig.MarkdownPipeline markdownPipeline, bool includeDrafts)
        {
            // Reset the current index, while tracking old data to detect update conflicts.
            this.rawDataFiles.Clear();
            this.staticFiles.Clear();

            var oldPages = this.pages;
            this.pages = new Dictionary<string, PageData>();

            var oldPosts = this.posts;
            this.posts = new Dictionary<string, PostData>();

            var oldCategories = this.categories;
            this.categories = new Dictionary<string, CategoryData>();

            var oldGames = this.games;
            this.games = new Dictionary<string, GameData>();

            var oldPlatforms = this.platforms;
            this.platforms = new Dictionary<string, PlatformData>();

            var oldRatings = this.ratings;
            this.ratings = new Dictionary<string, RatingData>();

            var oldTags = this.tags;
            this.tags = new Dictionary<string, TagData>();

            // Load game data from the IGDB cache.
            foreach (var igdbGame in igdbCache.GetAllGames())
            {
                var gameData = GameData.FromIgdbGame(igdbCache, igdbGame, this);

                this.games.Add(gameData.GetDataId(), gameData);
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
            }

            foreach (var igdbGameMetadata in igdbCache.GetAllGameMetadata())
            {
                var tagType = igdbGameMetadata.GetType();
                var tagName = igdbGameMetadata.GetReferenceString(igdbCache);

                CreateOrMergeMultiKeyReferenceableData(this.tags, tagType, tagName);
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

                                foreach (var linkToOtherGameName in gameData.LinkPostsToOtherGames)
                                {
                                    var otherGameData = this.GetGame(linkToOtherGameName);
                                    otherGameData.LinkedPostIds.Add(postId);
                                }

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
                this.CreateReference<GameData>(gameName);
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
