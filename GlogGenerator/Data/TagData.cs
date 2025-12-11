using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlogGenerator.IgdbApi;

namespace GlogGenerator.Data
{
    public class TagData : IGlogMultiKeyReferenceable
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
        private List<Tuple<Type, string>> referenceableTypedKeys = new List<Tuple<Type, string>>();

        public TagData(string tagName)
        {
            var igdbReference = new IgdbMetadataReference(tagName);
            this.igdbReferences.Add(igdbReference);

            // FIXME: deduplicate these internal properties (the list of igdbReferences should be sufficient).
            var typedKey = new Tuple<Type, string>(typeof(IgdbEntity), tagName);
            this.referenceableTypedKeys.Add(typedKey);
        }

        public TagData(IgdbMetadataReference igdbReference)
        {
            this.igdbReferences.Add(igdbReference);

            // FIXME: deduplicate these internal properties (the list of igdbReferences should be sufficient).
            var typedKey = new Tuple<Type, string>(igdbReference.IgdbEntityType, igdbReference.Name);
            this.referenceableTypedKeys.Add(typedKey);
        }

        public TagData(IEnumerable<IgdbMetadataReference> igdbReferences)
        {
            foreach (var igdbReference in igdbReferences)
            {
                this.igdbReferences.Add(igdbReference);

                // FIXME: deduplicate these internal properties (the list of igdbReferences should be sufficient).
                var typedKey = new Tuple<Type, string>(igdbReference.IgdbEntityType, igdbReference.Name);
                this.referenceableTypedKeys.Add(typedKey);
            }
        }

        public bool MatchesReferenceableKey(string matchKey)
        {
            return this.igdbReferences.Select(r => r.GetReferenceableKey()).Contains(matchKey);
        }

        public void MergeReferenceableKey(Type mergeKeyType, string mergeKey)
        {
            // FIXME: This should search for an existing IgdbReference instead!
            var typedKey = new Tuple<Type, string>(mergeKeyType, mergeKey);
            if (!this.referenceableTypedKeys.Contains(typedKey))
            {
                this.referenceableTypedKeys.Add(typedKey);
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
            }
        }

        public void AddIgdbMetadataReference(IgdbMetadataReference igdbReference)
        {
            this.igdbReferences.Add(igdbReference);
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
