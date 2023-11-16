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
                if (frontMatter != null && frontMatter.ContainsKey("permalink"))
                {
                    var frontMatterPermalink = (string)frontMatter["permalink"];
                    if (!string.IsNullOrEmpty(frontMatterPermalink))
                    {
                        if (frontMatterPermalink.StartsWith('/'))
                        {
                            frontMatterPermalink = frontMatterPermalink.Substring(1);
                        }

                        if (!frontMatterPermalink.EndsWith('/'))
                        {
                            frontMatterPermalink += '/';
                        }

                        return frontMatterPermalink;
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

        private Tomlyn.Model.TomlTable GetFrontMatter()
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
