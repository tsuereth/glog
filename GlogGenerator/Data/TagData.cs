using System;
using System.Collections.Generic;
using System.Linq;

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

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        private SortedSet<string> referenceableKeys = new SortedSet<string>(StringComparer.Ordinal);

        public TagData(string gameMetadataName) : base(gameMetadataName)
        {
            this.referenceableKeys.Add(gameMetadataName);
        }

        public void MergeReferenceableKey(string mergeKey)
        {
            this.referenceableKeys.Add(mergeKey);
        }

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.Name);
            return $"tag/{urlized}/";
        }
    }
}
