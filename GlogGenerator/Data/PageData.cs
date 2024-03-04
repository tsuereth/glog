using System;
using System.Globalization;
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

        public string PermalinkRelative { get { return this.permalinkRelative; } }

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

        private string permalinkRelative = null;

        public static string PageIdFromFilePath(MarkdownPipeline mdPipeline, string filePath)
        {
            // We need to parse the file to determine its permalink, based on front matter data.
            var fileContent = File.ReadAllText(filePath);
            var mdDoc = Markdown.Parse(fileContent, mdPipeline);

            string permalinkRelative = null;
            var frontMatterBlock = mdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();
            if (frontMatterBlock != null)
            {
                var frontMatter = frontMatterBlock.GetModel();

                if (frontMatter.TryGetNode("permalink", out var frontMatterPermalink))
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

                        permalinkRelative = permalinkString;
                    }
                }
            }

            return permalinkRelative;
        }

        public static PageData MarkdownFromFilePath(MarkdownPipeline mdPipeline, string filePath)
        {
            var text = File.ReadAllText(filePath);

            var page = new PageData();
            page.SourceFilePath = filePath;

            page.MdDoc = Markdown.Parse(text, mdPipeline);

            var frontMatterBlock = page.MdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();
            if (frontMatterBlock != null)
            {
                var frontMatter = frontMatterBlock.GetModel();
            
                if (frontMatter.TryGetNode("permalink", out var frontMatterPermalink))
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

                        page.permalinkRelative = permalinkString;
                    }
                }
            }

            return page;
        }
    }
}
