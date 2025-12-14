using System;
using System.Collections.Generic;
using System.Linq;
using GlogGenerator.Data;
using GlogGenerator.IgdbApi;

namespace GlogGenerator.Test
{
    public class FakeSiteDataIndex : ISiteDataIndex
    {
        private List<CategoryData> categories = new List<CategoryData>();
        private List<GameData> games = new List<GameData>();
        private List<PlatformData> platforms = new List<PlatformData>();
        private List<RatingData> ratings = new List<RatingData>();
        private List<TagData> tags = new List<TagData>();

        public SiteDataReference<T> CreateReference<T>(string referenceKey, bool shouldUpdateOnDataChange)
            where T : IGlogReferenceable
        {
            return new SiteDataReference<T>(referenceKey, shouldUpdateOnDataChange);
        }

        public T GetData<T>(SiteDataReference<T> dataReference)
            where T : class, IGlogReferenceable
        {
            var dataType = typeof(T);
            T data;

            List<T> dataLookup;
            if (dataType == typeof(CategoryData))
            {
                dataLookup = this.categories as List<T>;
            }
            else if (dataType == typeof(GameData))
            {
                dataLookup = this.games as List<T>;
            }
            else if (dataType == typeof(PlatformData))
            {
                dataLookup = this.platforms as List<T>;
            }
            else if (dataType == typeof(RatingData))
            {
                dataLookup = this.ratings as List<T>;
            }
            else if (dataType == typeof(TagData))
            {
                dataLookup = this.tags as List<T>;
            }
            else
            {
                throw new NotImplementedException($"Missing lookup for type {dataType.Name}");
            }

            // In this fake index, references are NEVER resolved,
            // because there are no data IDs and resolved references mean nothing.
            if (dataReference.GetIsResolved())
            {
                throw new ArgumentException($"The provided reference is resolved (ID {dataReference.GetResolvedReferenceId()}, which is not valid for FakeSiteDataIndex");
            }

            var referenceKey = dataReference.GetUnresolvedReferenceKey();

            data = dataLookup.Where(v => v.MatchesReferenceableKey(referenceKey)).FirstOrDefault();
            if (data == null)
            {
                // Invent the requested data.
                if (dataType == typeof(CategoryData))
                {
                    var categoryData = new CategoryData() { Name = referenceKey };
                    this.categories.Add(categoryData);
                    data = categoryData as T;
                }
                else if (dataType == typeof(GameData))
                {
                    var gameData = new GameData() { Title = referenceKey };
                    this.games.Add(gameData);
                    data = gameData as T;
                }
                else if (dataType == typeof(PlatformData))
                {
                    var platformData = new PlatformData() { Abbreviation = referenceKey };
                    this.platforms.Add(platformData);
                    data = platformData as T;
                }
                else if (dataType == typeof(RatingData))
                {
                    var ratingData = new RatingData() { Name = referenceKey };
                    this.ratings.Add(ratingData);
                    data = ratingData as T;
                }
                else if (dataType == typeof(TagData))
                {
                    var tagData = new TagData(referenceKey);
                    this.tags.Add(tagData);
                    data = tagData as T;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return data;
        }

        public T GetDataWithOldLookup<T>(SiteDataReference<T> dataReference, GlogReferenceableLookup<T> oldDataLookup)
            where T : class, IGlogReferenceable
        {
            throw new NotImplementedException();
        }

        public List<CategoryData> GetCategories()
        {
            return this.categories.ToList();
        }

        public bool HasGame(string gameTitle)
        {
            return this.games.Where(d => d.Title.Equals(gameTitle, StringComparison.Ordinal)).Any();
        }

        public GameData GetGame(string gameTitle)
        {
            var game = this.games.Where(d => d.Title.Equals(gameTitle, StringComparison.Ordinal)).FirstOrDefault();
            if (game == null)
            {
                throw new ArgumentException($"No game found for title {gameTitle}");
            }

            return game;
        }

        public List<GameData> GetGames()
        {
            return this.games.ToList();
        }

        public List<PageData> GetPages()
        {
            throw new NotImplementedException();
        }

        public PlatformData GetPlatform(string platformAbbreviation)
        {
            var platform = this.platforms.Where(d => d.Abbreviation.Equals(platformAbbreviation, StringComparison.Ordinal)).FirstOrDefault();
            if (platform == null)
            {
                throw new ArgumentException($"No platform found for abbreviation {platformAbbreviation}");
            }

            return platform;
        }

        public List<PlatformData> GetPlatforms()
        {
            return this.platforms.ToList();
        }

        public PostData GetPostById(string postId)
        {
            throw new NotImplementedException();
        }

        public List<PostData> GetPosts()
        {
            throw new NotImplementedException();
        }

        public List<RatingData> GetRatings()
        {
            return this.ratings.ToList();
        }

        public string GetRawDataFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public List<StaticFileData> GetStaticFiles()
        {
            throw new NotImplementedException();
        }

        public TagData GetTag(string tagName)
        {
            var tag = this.tags.Where(d => d.MatchesReferenceableKey(tagName)).FirstOrDefault();
            if (tag == null)
            {
                throw new ArgumentException($"No tag found for name {tagName}");
            }

            return tag;
        }

        public List<TagData> GetTags()
        {
            return this.tags.ToList();
        }

        public void SetNonContentGameNames(List<string> gameNames)
        {
            // no-op
        }

        public void LoadContent(IIgdbCache igdbCache, ContentParser contentParser, bool includeDrafts)
        {
            throw new NotImplementedException();
        }

        public void ResolveReferences()
        {
            throw new NotImplementedException();
        }

        public void RegisterAutomaticReferences(IIgdbCache igdbCache)
        {
            throw new NotImplementedException();
        }

        public void RemoveUnreferencedData(IIgdbCache igdbCache)
        {
            throw new NotImplementedException();
        }

        public void LinkPostsToAssociatedGames()
        {
            throw new NotImplementedException();
        }

        public void RewriteSourceContent(ContentParser contentParser)
        {
            throw new NotImplementedException();
        }

        public void WriteToJsonFiles(string directoryPath)
        {
            throw new NotImplementedException();
        }
    }
}
