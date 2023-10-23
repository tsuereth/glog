using System.Collections.Generic;
using GlogGenerator.IgdbApi;

namespace GlogGenerator.Data
{
    public class PlatformData : IGlogReferenceable
    {
        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.Abbreviation);
            return $"platform/{urlized}/";
        }

        public string Abbreviation { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string IgdbUrl { get; set; }

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        public static PlatformData FromIgdbPlatform(IgdbCache igdbCache, IgdbPlatform igdbPlatform)
        {
            var platform = new PlatformData();

            platform.Abbreviation = igdbPlatform.AbbreviationForGlog;
            platform.Name = igdbPlatform.Name;

            if (!string.IsNullOrEmpty(igdbPlatform.Url))
            {
                platform.IgdbUrl = igdbPlatform.Url;
            }

            return platform;
        }
    }
}
