using System.IO;
using System.Runtime.Serialization;
using System.Text;
using GlogGenerator.MarkdownExtensions;
using Markdig;

namespace GlogGenerator.Data
{
    public class PageData : ContentWithFrontMatterData
    {
        public static readonly string PageContentBaseDir = "content";

        [IgnoreDataMember]
        public string SourceFilePath { get; private set; } = string.Empty;

        [DataMember(Name = "permalink")]
        public string PermalinkRelative { get; private set; } = string.Empty;

        public void RewriteSourceFile(MarkdownPipeline mdPipeline)
        {
            if (string.IsNullOrEmpty(SourceFilePath))
            {
                throw new InvalidDataException("SourceFilePath is empty");
            }

            var fileContent = this.ToMarkdownString(mdPipeline);

            var utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            File.WriteAllText(this.SourceFilePath, fileContent, utf8WithoutBom);
        }

        public static PageData MarkdownFromFilePath(MarkdownPipeline mdPipeline, string filePath)
        {
            var page = ContentWithFrontMatterData.FromFilePath<PageData>(mdPipeline, filePath);
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
