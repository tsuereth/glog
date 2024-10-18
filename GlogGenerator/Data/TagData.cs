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
                return this.GetAllReferenceableKeys().FirstOrDefault();
            }
        }

        public HashSet<string> LinkedPostIds { get; set; } = new HashSet<string>();

        private List<Tuple<Type, string>> referenceableTypedKeys = new List<Tuple<Type, string>>();

        public TagData(Type gameMetadataType, string gameMetadataName) : base(gameMetadataType, gameMetadataName)
        {
            var typedKey = new Tuple<Type, string>(gameMetadataType, gameMetadataName);
            this.referenceableTypedKeys.Add(typedKey);
        }

        public bool MatchesReferenceableKey(string matchKey)
        {
            return this.GetAllReferenceableKeys().Contains(matchKey);
        }

        public bool ShouldMergeWithReferenceableKey(string checkKey)
        {
            var thisKeyUrlized = new UrlizedString(this.Name);
            var checkKeyUrlized = new UrlizedString(checkKey);

            return thisKeyUrlized.Equals(checkKeyUrlized);
        }

        public void MergeReferenceableKey(Type mergeKeyType, string mergeKey)
        {
            var typedKey = new Tuple<Type, string>(mergeKeyType, mergeKey);
            this.referenceableTypedKeys.Add(typedKey);
        }

        public string GetDataId()
        {
            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var typeBytes = Encoding.UTF8.GetBytes(nameof(TagData));
                hash.AppendData(typeBytes);

                var nameUrlized = UrlizedString.Urlize(this.Name);
                var nameUrlizedBytes = Encoding.UTF8.GetBytes(nameUrlized);
                hash.AppendData(nameUrlizedBytes);

                var idBytes = hash.GetCurrentHash();
                return Convert.ToHexString(idBytes);
            }
        }

        public string GetReferenceableKey()
        {
            return this.Name;
        }

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.GetReferenceableKey());
            return $"tag/{urlized}/";
        }

        private List<string> GetAllReferenceableKeys()
        {
            return this.referenceableTypedKeys.Select(t => t.Item2).Distinct().OrderBy(s => s).ToList();
        }

        public List<Tuple<Type, string>> GetReferenceableTypedKeys()
        {
            return this.referenceableTypedKeys;
        }
    }
}
