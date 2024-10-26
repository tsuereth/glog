using System;
using GlogGenerator.Data;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogLinkInline : LinkInline
    {
        public string ReferenceTypeName { get; private set; }

        private readonly string originalReferenceKey;
        private readonly ISiteDataReference dataReference;

        public GlogLinkInline(string referenceTypeName, string referenceKey, ISiteDataIndex siteDataIndex)
        {
            this.ReferenceTypeName = referenceTypeName;
            this.originalReferenceKey = referenceKey;

            if (siteDataIndex == null)
            {
                this.dataReference = null;
            }
            else
            {
                switch (referenceTypeName)
                {
                    case "category":
                        this.dataReference = siteDataIndex.CreateReference<CategoryData>(referenceKey, true) as ISiteDataReference;
                        break;

                    case "game":
                        this.dataReference = siteDataIndex.CreateReference<GameData>(referenceKey, true) as ISiteDataReference;
                        break;

                    case "platform":
                        this.dataReference = siteDataIndex.CreateReference<PlatformData>(referenceKey, true) as ISiteDataReference;
                        break;

                    case "rating":
                        this.dataReference = siteDataIndex.CreateReference<RatingData>(referenceKey, true) as ISiteDataReference;
                        break;

                    case "tag":
                        this.dataReference = siteDataIndex.CreateReference<TagData>(referenceKey, true) as ISiteDataReference;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public string GetReferenceKey(ISiteDataIndex siteDataIndex)
        {
            if (this.dataReference == null)
            {
                return this.originalReferenceKey;
            }

            IGlogReferenceable data;
            switch (this.ReferenceTypeName)
            {
                case "category":
                    var categoryReference = this.dataReference as SiteDataReference<CategoryData>;
                    data = siteDataIndex.GetData<CategoryData>(categoryReference);
                    break;

                case "game":
                    var gameReference = this.dataReference as SiteDataReference<GameData>;
                    data = siteDataIndex.GetData<GameData>(gameReference);
                    break;

                case "platform":
                    var platformReference = this.dataReference as SiteDataReference<PlatformData>;
                    data = siteDataIndex.GetData<PlatformData>(platformReference);
                    break;

                case "rating":
                    var ratingReference = this.dataReference as SiteDataReference<RatingData>;
                    data = siteDataIndex.GetData<RatingData>(ratingReference);
                    break;

                case "tag":
                    var tagReference = this.dataReference as SiteDataReference<TagData>;
                    data = siteDataIndex.GetData<TagData>(tagReference);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (data == null)
            {
                throw new ArgumentException($"No {this.ReferenceTypeName} found with key {this.dataReference.GetUnresolvedReferenceKey()}");
            }

            return data.GetReferenceableKey();
        }
    }
}
