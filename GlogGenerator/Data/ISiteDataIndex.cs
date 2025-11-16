using System.Collections.Generic;

namespace GlogGenerator.Data
{
    public interface ISiteDataIndex
    {
        public SiteDataReference<T> CreateReference<T>(string referenceKey, bool shouldUpdateOnDataChange)
            where T : IGlogReferenceable;

        public T GetData<T>(SiteDataReference<T> dataReference)
            where T : class, IGlogReferenceable;

        public T GetDataWithOldLookup<T>(SiteDataReference<T> dataReference, Dictionary<string, T> oldDataLookup)
            where T : class, IGlogReferenceable;

        public List<CategoryData> GetCategories();

        public bool HasGame(string gameTitle);

        public GameData GetGame(string gameTitle);

        public List<GameData> GetGames();

        public List<PageData> GetPages();

        public PlatformData GetPlatform(string platformAbbreviation);

        public List<PlatformData> GetPlatforms();

        public PostData GetPostById(string postId);

        public List<PostData> GetPosts();

        public List<RatingData> GetRatings();

        public string GetRawDataFile(string filePath);

        public List<StaticFileData> GetStaticFiles();

        public TagData GetTag(string tagName);

        public List<TagData> GetTags();

        public void SetNonContentGameNames(List<string> gameNames);

        public void LoadContent(IIgdbCache igdbCache, Markdig.MarkdownPipeline markdownHtmlPipeline, Markdig.MarkdownPipeline markdownRoundtripPipeline, bool includeDrafts);

        public void ResolveReferences();

        public void RegisterAutomaticReferences(IIgdbCache igdbCache);

        public void RemoveUnreferencedData(IIgdbCache igdbCache);

        public void LinkPostsToAssociatedGames();

        public void RewriteSourceContent(Markdig.MarkdownPipeline markdownPipeline);
    }
}
