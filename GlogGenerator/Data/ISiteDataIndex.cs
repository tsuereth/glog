using System.Collections.Generic;

namespace GlogGenerator.Data
{
    public interface ISiteDataIndex
    {
        public List<CategoryData> GetCategories();

        public GameData GetGame(string gameTitle);

        public List<GameData> GetGames();

        public List<PageData> GetPages();

        public PlatformData GetPlatform(string platformAbbreviation);

        public List<PlatformData> GetPlatforms();

        public List<PostData> GetPosts();

        public List<RatingData> GetRatings();

        public string GetRawDataFile(string filePath);

        public List<StaticFileData> GetStaticFiles();

        public TagData GetTag(string tagName);

        public List<TagData> GetTags();

        public void LoadContent(IIgdbCache igdbCache);

        public void RewriteSourceContent();
    }
}
