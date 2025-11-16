using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

        public string SourceFileContentHash { get; private set; } = string.Empty;

        public string PermalinkRelative { get { return this.permalinkRelative; } }

        public void RewriteSourceFile(MarkdownPipeline mdPipeline)
        {
            if (string.IsNullOrEmpty(SourceFilePath))
            {
                throw new InvalidDataException("SourceFilePath is empty");
            }

            var fileContent = this.MdDocRoundtrippable.ToMarkdownString(mdPipeline);

            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var contentBytes = Encoding.UTF8.GetBytes(fileContent);
                hash.AppendData(contentBytes);
                var contentHashBytes = hash.GetCurrentHash();
                var contentHash = Convert.ToHexString(contentHashBytes);

                if (!contentHash.Equals(this.SourceFileContentHash, StringComparison.Ordinal))
                {
                    this.SourceFileContentHash = contentHash;

                    var utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                    File.WriteAllText(this.SourceFilePath, fileContent, utf8WithoutBom);
                }
            }
        }

        public MarkdownDocument MdDocHtmlRenderable { get; private set; }

        public MarkdownDocument MdDocRoundtrippable { get; private set; }

        private string permalinkRelative = null;

        public static string PageIdFromFilePath(MarkdownPipeline mdPipeline, string filePath)
        {
            // Disable the data index in this file parse, so that it doesn't create unwanted data references.
            var parseContext = MarkdownParserContextExtensions.DontUseSiteDataIndex();

            // We need to parse the file to determine its permalink, based on front matter data.
            var fileContent = File.ReadAllText(filePath);
            var mdDoc = Markdown.Parse(fileContent, mdPipeline, parseContext);

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

        public static PageData MarkdownFromFilePath(MarkdownPipeline mdHtmlPipeline, MarkdownPipeline mdRoundtripPipeline, string filePath)
        {
            var text = File.ReadAllText(filePath);

            var page = new PageData();
            page.SourceFilePath = filePath;

            page.MdDocHtmlRenderable = Markdown.Parse(text, mdHtmlPipeline);
            page.MdDocRoundtrippable = Markdown.Parse(text, mdRoundtripPipeline);

            var frontMatterBlock = page.MdDocRoundtrippable.Descendants<TomlFrontMatterBlock>().FirstOrDefault();
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

            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var contentBytes = Encoding.UTF8.GetBytes(text);
                hash.AppendData(contentBytes);
                var contentHashBytes = hash.GetCurrentHash();
                page.SourceFileContentHash = Convert.ToHexString(contentHashBytes);
            }

            return page;
        }
    }
}
