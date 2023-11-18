using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GlogGenerator.Data
{
    public class TagData : GlogDataFromIgdbGameMetadata, IGlogMultiKeyReferenceable
    {
        public string Name
        {
            get
            {
                return this.referenceableKeys.FirstOrDefault();
            }
        }

        public List<string> LinkedPostIds { get; set; } = new List<string>();

        private SortedSet<string> referenceableKeys = new SortedSet<string>(StringComparer.Ordinal);

        public TagData(string gameMetadataName) : base(gameMetadataName)
        {
            this.referenceableKeys.Add(gameMetadataName);
        }

        public bool MatchesReferenceableKey(string matchKey)
        {
            return this.referenceableKeys.Contains(matchKey);
        }

        public bool ShouldMergeWithReferenceableKey(string checkKey)
        {
            var thisKeyUrlized = new UrlizedString(this.Name);
            var checkKeyUrlized = new UrlizedString(checkKey);

            return thisKeyUrlized.Equals(checkKeyUrlized);
        }

        public void MergeReferenceableKey(string mergeKey)
        {
            this.referenceableKeys.Add(mergeKey);
        }

        public string GetDataId()
        {
            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var typeBytes = Encoding.UTF8.GetBytes(nameof(TagData));
                hash.AppendData(typeBytes);

                var nameBytes = Encoding.UTF8.GetBytes(this.Name);
                hash.AppendData(nameBytes);

                var idBytes = hash.GetCurrentHash();
                return Convert.ToHexString(idBytes);
            }
        }

        public string GetReferenceableKey()
        {
            return UrlizedString.Urlize(this.Name);
        }

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.GetReferenceableKey());
            return $"tag/{urlized}/";
        }
    }
}
