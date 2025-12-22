using System;
using System.Collections.Generic;
using System.IO;
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

        public IEnumerable<string> GetIgdbEntityReferenceIds()
        {
            if (this.igdbReference != null && this.igdbReference.HasIgdbEntityData())
            {
                return new List<string>() { this.igdbReference.GetIgdbEntityDataId() };
            }

            return new List<string>();
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

            // FIXME: PlatformData `Abbreviation` and `Name` should be disambiguated...
            // - Typically, a platform is *referenced by* its abbreviation -- or by name as a fallback.
            // - But when PlatformData is used to create a renderable Page, both its name and abbreviation may be shown at once.
            platform.Abbreviation = platform.igdbReference.GetReferenceableKey();
            platform.Name = platform.igdbReference.GetReferenceableKey();

            return platform;
        }

        public void PopulateRelatedIgdbData(IIgdbCache igdbCache)
        {
            if (!this.igdbReference.HasIgdbEntityData())
            {
                return;
            }

            var igdbPlatform = igdbCache.GetPlatform(this.igdbReference.IgdbEntityId.Value);
            if (igdbPlatform == null)
            {
                throw new InvalidDataException($"No IGDB Platform found with ID {this.igdbReference.IgdbEntityId.Value}");
            }

            this.Abbreviation = igdbPlatform.Abbreviation;
            this.Name = igdbPlatform.Name;

            if (!string.IsNullOrEmpty(igdbPlatform.Url))
            {
                this.IgdbUrl = igdbPlatform.Url;
            }
        }
    }
}
