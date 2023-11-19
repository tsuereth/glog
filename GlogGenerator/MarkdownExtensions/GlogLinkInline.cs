using System;
using GlogGenerator.Data;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogLinkInline : LinkInline
    {
        public string ReferenceTypeName { get; private set; }

        public ISiteDataReference DataReference { get; private set; }

        public GlogLinkInline(string referenceTypeName, string referenceKey, ISiteDataIndex siteDataIndex)
        {
            this.ReferenceTypeName = referenceTypeName;
            switch (referenceTypeName)
            {
                case "category":
                    this.DataReference = siteDataIndex.CreateReference<CategoryData>(referenceKey) as ISiteDataReference;
                    break;

                case "game":
                    this.DataReference = siteDataIndex.CreateReference<GameData>(referenceKey) as ISiteDataReference;
                    break;

                case "platform":
                    this.DataReference = siteDataIndex.CreateReference<PlatformData>(referenceKey) as ISiteDataReference;
                    break;

                case "rating":
                    this.DataReference = siteDataIndex.CreateReference<RatingData>(referenceKey) as ISiteDataReference;
                    break;

                case "tag":
                    this.DataReference = siteDataIndex.CreateReference<TagData>(referenceKey) as ISiteDataReference;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public string GetReferenceKey(ISiteDataIndex siteDataIndex)
        {
            IGlogReferenceable data;
            switch (this.ReferenceTypeName)
            {
                case "category":
                    var categoryReference = this.DataReference as SiteDataReference<CategoryData>;
                    data = siteDataIndex.GetData<CategoryData>(categoryReference);
                    break;

                case "game":
                    var gameReference = this.DataReference as SiteDataReference<GameData>;
                    data = siteDataIndex.GetData<GameData>(gameReference);
                    break;

                case "platform":
                    var platformReference = this.DataReference as SiteDataReference<PlatformData>;
                    data = siteDataIndex.GetData<PlatformData>(platformReference);
                    break;

                case "rating":
                    var ratingReference = this.DataReference as SiteDataReference<RatingData>;
                    data = siteDataIndex.GetData<RatingData>(ratingReference);
                    break;

                case "tag":
                    var tagReference = this.DataReference as SiteDataReference<TagData>;
                    data = siteDataIndex.GetData<TagData>(tagReference);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (data == null)
            {
                throw new ArgumentException($"No {this.ReferenceTypeName} found with key {this.DataReference.GetUnresolvedReferenceKey()}");
            }

            return data.GetReferenceableKey();
        }
    }
}
