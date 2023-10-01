using System.Collections.Generic;
using GlogGenerator.IgdbApi;
using GlogGenerator.TemplateRenderers;

namespace GlogGenerator.Data
{
    public class PlatformData
    {
        public string PermalinkRelative
        {
            get
            {
                var urlized = StringRenderer.Urlize(this.Name);
                return $"platform/{urlized}/";
            }
        }

        public string Name { get; set; } = string.Empty;

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        public static PlatformData FromIgdbPlatform(IgdbCache igdbCache, IgdbPlatform igdbPlatform)
        {
            var platform = new PlatformData();

            // FIXME: Distinguish 'Name' from 'Abbreviation'!
            platform.Name = igdbPlatform.AbbreviationForGlog;

            // FIXME: Set an IgdbUrl property!

            return platform;
        }
    }
}
