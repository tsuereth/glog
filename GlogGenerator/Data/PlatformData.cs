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

        public HashSet<string> LinkedPostIds { get; set; } = new HashSet<string>();

        private IgdbPlatformReference igdbReference;
        private string dataId;
        private string referenceableKey;

        public string GetDataId()
        {
            return this.dataId;
        }

        public string GetReferenceableKey()
        {
            if (!string.IsNullOrEmpty(this.referenceableKey))
            {
                return this.referenceableKey;
            }

            return this.Abbreviation;
        }

        public bool MatchesReferenceableKey(string matchKey)
        {
            var thisKey = this.GetReferenceableKey();
            return thisKey.Equals(matchKey, StringComparison.Ordinal);
        }

        public object GetReferenceProperties()
        {
            return this.igdbReference;
        }

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.GetReferenceableKey());
            return $"platform/{urlized}/";
        }

        public static PlatformData FromIgdbPlatform(IIgdbCache igdbCache, IgdbPlatform igdbPlatform)
        {
            var platform = new PlatformData();
            platform.igdbReference = new IgdbPlatformReference(igdbPlatform);
            platform.dataId = igdbPlatform.GetUniqueIdString(igdbCache);
            platform.referenceableKey = igdbPlatform.GetReferenceString(igdbCache);

            platform.Abbreviation = igdbPlatform.GetReferenceString(igdbCache);
            platform.Name = igdbPlatform.Name;

            if (!string.IsNullOrEmpty(igdbPlatform.Url))
            {
                platform.IgdbUrl = igdbPlatform.Url;
            }

            return platform;
        }
    }
}
