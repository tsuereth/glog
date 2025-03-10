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
            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var typeBytes = Encoding.UTF8.GetBytes(nameof(CategoryData));
                hash.AppendData(typeBytes);

                var nameBytes = Encoding.UTF8.GetBytes(this.Name);
                hash.AppendData(nameBytes);

                var idBytes = hash.GetCurrentHash();
                return Convert.ToHexString(idBytes);
            }
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

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.GetReferenceableKey());
            return $"category/{urlized}/";
        }
    }
}
