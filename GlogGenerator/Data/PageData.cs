using GlogGenerator.MarkdownExtensions;

namespace GlogGenerator.Data
{
    public class PageData
    {
        public static readonly string PageContentBaseDir = "content";

        public string SourceFilePath { get; private set; } = string.Empty;

        public string PermalinkRelative { get; private set; } = string.Empty;

        public string Content { get; private set; } = string.Empty;

        public static PageData FromFilePath(string filePath)
        {
            var data = ContentWithFrontMatterData.FromFilePath(filePath);

            var page = new PageData();
            page.SourceFilePath = filePath;

            page.PermalinkRelative = data.GetValue<string>("permalink") ?? string.Empty;

            if (page.PermalinkRelative.StartsWith('/'))
            {
                page.PermalinkRelative = page.PermalinkRelative.Substring(1);
            }

            if (!page.PermalinkRelative.EndsWith('/'))
            {
                page.PermalinkRelative += '/';
            }

            page.Content = data.Content;

            return page;
        }
    }
}
