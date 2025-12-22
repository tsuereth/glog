using System;
using System.Collections.Generic;
using System.Text;

namespace GlogGenerator.Data
{
    public class RatingData : IGlogReferenceable
    {
        public string Name { get; set; } = string.Empty;

        public HashSet<string> LinkedPostIds { get; set; } = new HashSet<string>();

        public string GetDataId()
        {
            // Ratings have no backing metadata, they're identified simply by name.
            return $"{nameof(RatingData)}:name={this.Name}";
        }

        public string GetReferenceableKey()
        {
            return this.Name;
        }

        public bool MatchesReferenceableKey(string matchKey)
        {
            var thisKey = this.GetReferenceableKey();
            return thisKey.Equals(matchKey, StringComparison.Ordinal);
        }

        public IEnumerable<string> GetIgdbEntityReferenceIds()
        {
            // Ratings have no backing IGDB entity.
            return new List<string>();
        }

        public object GetReferenceProperties()
        {
            return this.Name;
        }

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.Name);
            return $"rating/{urlized}/";
        }
    }
}
