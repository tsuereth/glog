using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace GlogGenerator.Data
{
    public class CategoryData : IGlogReferenceable
    {
        // This category string is special, and frequently referenced programmatically.
        public const string PlayingAGameCategoryName = "Playing A Game";

        public string Name { get; set; } = string.Empty;

        public HashSet<string> LinkedPostIds { get; set; } = new HashSet<string>();

        public string GetDataId()
        {
            // Categories have no backing metadata, they're identified simply by name.
            return $"{nameof(CategoryData)}:name={this.Name}";
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

        public object GetReferenceProperties()
        {
            return this.Name;
        }

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.GetReferenceableKey());
            return $"category/{urlized}/";
        }
    }
}
