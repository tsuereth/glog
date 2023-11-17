using System.IO;
using System.Linq;
using System.Text;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Markdig.Syntax;

namespace GlogGenerator.Data
{
    public class PageData
    {
        public static readonly string PageContentBaseDir = "content";

        public string SourceFilePath { get; private set; } = string.Empty;

        public string PermalinkRelative
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.TryGetNode("permalink", out var frontMatterPermalink))
                {
                    var permalinkString = frontMatterPermalink.ToString();
                    if (!string.IsNullOrEmpty(permalinkString))
                    {
                        if (permalinkString.StartsWith('/'))
                        {
                            permalinkString = permalinkString.Substring(1);
                        }

                        if (!permalinkString.EndsWith('/'))
                        {
                            permalinkString += '/';
                        }

                        return permalinkString;
                    }
                }

                return string.Empty;
            }
        }

        public void RewriteSourceFile(MarkdownPipeline mdPipeline)
        {
            if (string.IsNullOrEmpty(SourceFilePath))
            {
                throw new InvalidDataException("SourceFilePath is empty");
            }

            var fileContent = this.MdDoc.ToMarkdownString(mdPipeline);

            var utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            File.WriteAllText(this.SourceFilePath, fileContent, utf8WithoutBom);
        }

        public MarkdownDocument MdDoc { get; private set; }

        private Tommy.TomlTable GetFrontMatter()
        {
            var frontMatterBlock = this.MdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();
            if (frontMatterBlock != null)
            {
                return frontMatterBlock.GetModel();
            }

            return null;
        }

        public static PageData MarkdownFromFilePath(MarkdownPipeline mdPipeline, string filePath)
        {
            var text = File.ReadAllText(filePath);

            var page = new PageData();
            page.SourceFilePath = filePath;

            page.MdDoc = Markdown.Parse(text, mdPipeline);

            return page;
        }
    }
}
