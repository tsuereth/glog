using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlogGenerator.Data
{
    public class TagData : IGlogReferenceable
    {
        public string Name
        {
            get
            {
                return this.igdbReferences.Select(r => r.GetReferenceableKey()).OrderBy(n => n, StringComparer.Ordinal).FirstOrDefault();
            }
        }

        public HashSet<string> LinkedPostIds { get; set; } = new HashSet<string>();

        private List<IgdbMetadataReference> igdbReferences = new List<IgdbMetadataReference>();

        public TagData(string tagName)
        {
            var igdbReference = new IgdbMetadataReference(tagName);
            this.igdbReferences.Add(igdbReference);
        }

        public TagData(IgdbMetadataReference igdbReference)
        {
            this.igdbReferences.Add(igdbReference);
        }

        public TagData(IEnumerable<IgdbMetadataReference> igdbReferences)
        {
            foreach (var igdbReference in igdbReferences)
            {
                this.igdbReferences.Add(igdbReference);
            }
        }

        public bool MatchesReferenceableKey(string matchKey)
        {
            var referenceableKeyUrlized = UrlizedString.Urlize(this.GetReferenceableKey());
            var matchKeyUrlized = UrlizedString.Urlize(matchKey);
            return referenceableKeyUrlized.Equals(matchKeyUrlized, StringComparison.Ordinal);
        }

        public IEnumerable<IgdbMetadataReference> GetIgdbEntityReferences()
        {
            return this.igdbReferences.Where(r => r.HasIgdbEntityData());
        }

        public string GetDataId()
        {
            // FIXME: This should be deprecated or renamed, "data id" is ambiguous here since there may be multiple entities.
            var nameUrlized = UrlizedString.Urlize(this.Name);
            return $"{nameof(TagData)}:key={nameUrlized}";
        }

        public string GetReferenceableKey()
        {
            return this.Name;
        }

        public IEnumerable<string> GetIgdbEntityReferenceIds()
        {
            return this.igdbReferences.Where(r => r.HasIgdbEntityData()).Select(r => r.GetIgdbEntityDataId());
        }

        public object GetReferenceProperties()
        {
            return this.igdbReferences;
        }

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.GetReferenceableKey());
            return $"tag/{urlized}/";
        }
    }
}
