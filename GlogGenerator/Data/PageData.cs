using System.Runtime.Serialization;
using GlogGenerator.MarkdownExtensions;

namespace GlogGenerator.Data
{
    public class PageData : ContentWithFrontMatterData
    {
        public static readonly string PageContentBaseDir = "content";

        [IgnoreDataMember]
        public string SourceFilePath { get; private set; } = string.Empty;

        [DataMember(Name = "permalink")]
        public string PermalinkRelative { get; private set; } = string.Empty;

        public static PageData FromFilePath(string filePath)
        {
            var page = ContentWithFrontMatterData.FromFilePath<PageData>(filePath);
            page.SourceFilePath = filePath;

            if (page.PermalinkRelative.StartsWith('/'))
            {
                page.PermalinkRelative = page.PermalinkRelative.Substring(1);
            }

            if (!page.PermalinkRelative.EndsWith('/'))
            {
                page.PermalinkRelative += '/';
            }

            return page;
        }
    }
}
