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
                return this.referenceableTypedKeyStringsCache.OrderBy(s => s, StringComparer.Ordinal).FirstOrDefault();
            }
        }

        public HashSet<string> LinkedPostIds { get; set; } = new HashSet<string>();

        private List<Tuple<Type, string>> referenceableTypedKeys = new List<Tuple<Type, string>>();
        private HashSet<string> referenceableTypedKeyStringsCache = new HashSet<string>();

        public TagData(Type gameMetadataType, string gameMetadataName) : base(gameMetadataType, gameMetadataName)
        {
            var typedKey = new Tuple<Type, string>(gameMetadataType, gameMetadataName);
            this.referenceableTypedKeys.Add(typedKey);
            this.referenceableTypedKeyStringsCache = this.referenceableTypedKeys.Select(t => t.Item2).ToHashSet();
        }

        public bool MatchesReferenceableKey(string matchKey)
        {
            return this.referenceableTypedKeyStringsCache.Contains(matchKey);
        }

        public void MergeReferenceableKey(Type mergeKeyType, string mergeKey)
        {
            var typedKey = new Tuple<Type, string>(mergeKeyType, mergeKey);
            if (!this.referenceableTypedKeys.Contains(typedKey))
            {
                this.referenceableTypedKeys.Add(typedKey);
                this.referenceableTypedKeyStringsCache = this.referenceableTypedKeys.Select(t => t.Item2).ToHashSet();
            }
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

        public List<Tuple<Type, string>> GetReferenceableTypedKeys()
        {
            return this.referenceableTypedKeys;
        }
    }
}
