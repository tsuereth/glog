using System;
using System.Collections.Generic;
using GlogGenerator.IgdbApi;

namespace GlogGenerator.Data
{
    public class PlatformData : IGlogReferenceable
    {
        public string Abbreviation { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string IgdbUrl { get; set; }

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        private string dataId;
        private string referenceableKey;

        public string GetDataId()
        {
            return this.dataId;
        }

        public string GetReferenceableKey()
        {
            return this.referenceableKey;
        }

        public string GetPermalinkRelative()
        {
            return $"platform/{this.referenceableKey}/";
        }

        public static PlatformData FromIgdbPlatform(IIgdbCache igdbCache, IgdbPlatform igdbPlatform)
        {
            var platform = new PlatformData();
            platform.dataId = igdbPlatform.GetUniqueIdString();
            platform.referenceableKey = igdbPlatform.GetReferenceableKey();

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
