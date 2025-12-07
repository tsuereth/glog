using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GlogGenerator.Data
{
    public class TagData : IGlogMultiKeyReferenceable
    {
        public string Name
        {
            get
            {
                return this.referenceableTypedKeyStringsCache.OrderBy(s => s, StringComparer.Ordinal).FirstOrDefault();
            }
        }

        public HashSet<string> LinkedPostIds { get; set; } = new HashSet<string>();

        private List<IgdbMetadataReference> igdbReferences = new List<IgdbMetadataReference>();
        private List<Tuple<Type, string>> referenceableTypedKeys = new List<Tuple<Type, string>>();
        private HashSet<string> referenceableTypedKeyStringsCache = new HashSet<string>();

        public TagData(Type gameMetadataType, string gameMetadataName)
        {
            // FIXME: deduplicate these internal properties (the list of igdbReferences should be sufficient).
            var typedKey = new Tuple<Type, string>(gameMetadataType, gameMetadataName);
            this.referenceableTypedKeys.Add(typedKey);
            this.referenceableTypedKeyStringsCache = this.referenceableTypedKeys.Select(t => t.Item2).ToHashSet();
        }

        public TagData(IgdbMetadataReference igdbReference)
        {
            this.igdbReferences.Add(igdbReference);

            // FIXME: deduplicate these internal properties (the list of igdbReferences should be sufficient).
            var typedKey = new Tuple<Type, string>(igdbReference.IgdbEntityType, igdbReference.Name);
            this.referenceableTypedKeys.Add(typedKey);
            this.referenceableTypedKeyStringsCache = this.referenceableTypedKeys.Select(t => t.Item2).ToHashSet();
        }

        public TagData(IEnumerable<IgdbMetadataReference> igdbReferences)
        {
            foreach (var igdbReference in igdbReferences)
            {
                this.igdbReferences.Add(igdbReference);

                // FIXME: deduplicate these internal properties (the list of igdbReferences should be sufficient).
                var typedKey = new Tuple<Type, string>(igdbReference.IgdbEntityType, igdbReference.Name);
                this.referenceableTypedKeys.Add(typedKey);
                this.referenceableTypedKeyStringsCache = this.referenceableTypedKeys.Select(t => t.Item2).ToHashSet();
            }
        }

        public bool MatchesReferenceableKey(string matchKey)
        {
            return this.referenceableTypedKeyStringsCache.Contains(matchKey);
        }

        public void MergeReferenceableKey(Type mergeKeyType, string mergeKey)
        {
            // FIXME: This should search for an existing IgdbReference instead!
            var typedKey = new Tuple<Type, string>(mergeKeyType, mergeKey);
            if (!this.referenceableTypedKeys.Contains(typedKey))
            {
                this.referenceableTypedKeys.Add(typedKey);
                this.referenceableTypedKeyStringsCache = this.referenceableTypedKeys.Select(t => t.Item2).ToHashSet();
            }
        }

        public void MergeIgdbMetadataReference(IgdbMetadataReference igdbReference)
        {
            // FIXME: This should search for an existing IgdbReference instead!
            var typedKey = new Tuple<Type, string>(igdbReference.IgdbEntityType, igdbReference.Name);
            if (!this.referenceableTypedKeys.Contains(typedKey))
            {
                this.igdbReferences.Add(igdbReference);

                this.referenceableTypedKeys.Add(typedKey);
                this.referenceableTypedKeyStringsCache = this.referenceableTypedKeys.Select(t => t.Item2).ToHashSet();
            }
        }

        public void AddIgdbMetadataReference(IgdbMetadataReference igdbReference)
        {
            this.igdbReferences.Add(igdbReference);
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

        public object GetReferenceProperties()
        {
            return this.igdbReferences;
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
