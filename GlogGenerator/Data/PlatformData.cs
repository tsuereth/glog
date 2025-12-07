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

        public IgdbPlatformReference GetIgdbEntityReference()
        {
            return igdbReference;
        }

        public string GetDataId()
        {
            if (this.igdbReference != null && this.igdbReference.HasIgdbEntityData())
            {
                return this.igdbReference.GetIgdbEntityDataId();
            }

            // When a backing data ID isn't available, there's not much choice left but the referenceable key.
            return $"{nameof(PlatformData)}:key={this.GetReferenceableKey()}";
        }

        public string GetReferenceableKey()
        {
            if (this.igdbReference != null)
            {
                return this.igdbReference.GetReferenceableKey();
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

        public static PlatformData FromIgdbPlatformReference(IgdbPlatformReference igdbPlatformReference)
        {
            var platform = new PlatformData();
            platform.igdbReference = igdbPlatformReference;

            return platform;
        }

        public static PlatformData FromIgdbPlatform(IIgdbCache igdbCache, IgdbPlatform igdbPlatform)
        {
            var platform = new PlatformData();
            platform.igdbReference = new IgdbPlatformReference(igdbPlatform);

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
