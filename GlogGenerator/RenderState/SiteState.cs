using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlogGenerator.Data;
using GlogGenerator.HugoCompat;

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

        public string OutputBasePath { get; set;} = "public";

        public Dictionary<string, PlatformData> Platforms { get; set; } = new Dictionary<string, PlatformData>();

        public Dictionary<string, RatingData> Ratings { get; set; } = new Dictionary<string, RatingData>();

        public Dictionary<string, TagData> Tags { get; set; } = new Dictionary<string, TagData>();

        public string Title { get; set; } = string.Empty;

        public FilePathResolver PathResolver { get; private set; }

        public SiteState(string basePath)
        {
            this.PathResolver = new FilePathResolver(basePath);
        }

        public CategoryData AddCategoryIfMissing(string categoryName, bool overwriteData = false)
        {
            var categoryKey = TemplateFunctionsStringRenderer.Urlize(categoryName, htmlEncode: false);

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

        public GameData AddGameIfMissing(string gameName, bool overwriteData = false)
        {
            var gameKey = TemplateFunctionsStringRenderer.Urlize(gameName, htmlEncode: false);

            if (!this.Games.ContainsKey(gameKey))
            {
                var newGame = new GameData()
                {
                    Title = gameName,
                };

                this.Games[gameKey] = newGame;
            }
            else if (overwriteData)
            {
                this.Games[gameKey].Title = gameName;
            }

            return this.Games[gameKey];
        }

        public PlatformData AddPlatformIfMissing(string platformName, bool overwriteData = false)
        {
            var platformKey = TemplateFunctionsStringRenderer.Urlize(platformName, htmlEncode: false);

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
            var ratingKey = TemplateFunctionsStringRenderer.Urlize(ratingName, htmlEncode: false);

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
            var tagKey = TemplateFunctionsStringRenderer.Urlize(tagName, htmlEncode: false);

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

        public static SiteState FromConfigData(ConfigData configData)
        {
            var site = new SiteState(configData.DataBasePath)
            {
                Author = configData.Author,
                BaseURL = configData.BaseURL,
                BuildDate = DateTimeOffset.Now,
                LanguageCode = configData.LanguageCode,
                NowPlaying = configData.NowPlaying,
                Title = configData.Title
            };

            site.OutputBasePath = Path.Combine(Directory.GetCurrentDirectory(), "public");

            return site;
        }
    }
}
