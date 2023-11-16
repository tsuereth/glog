using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Markdig.Syntax;

namespace GlogGenerator.Data
{
    public class PostData
    {
        public static readonly string PostContentBaseDir = "content/post";

        public string SourceFilePath { get; private set; } = string.Empty;

        public string PermalinkRelative
        {
            get
            {
                var permalinkPathParts = new List<string>(4)
                {
                    this.Date.Year.ToString("D4", CultureInfo.InvariantCulture),
                    this.Date.Month.ToString("D2", CultureInfo.InvariantCulture),
                    this.Date.Day.ToString("D2", CultureInfo.InvariantCulture),
                };

                var specifiedSlug = false;
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.ContainsKey("slug"))
                {
                    var slug = (string)frontMatter["slug"];
                    if (!string.IsNullOrEmpty(slug))
                    {
                        specifiedSlug = true;
                        permalinkPathParts.Add(slug);
                    }
                }

                if (!specifiedSlug)
                {
                    permalinkPathParts.Add(UrlizedString.Urlize(this.Title));
                }

                return string.Join('/', permalinkPathParts) + '/';
            }
        }

        public DateTimeOffset Date
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.ContainsKey("date"))
                {
                    var dateString = (string)frontMatter["date"];
                    if (!string.IsNullOrEmpty(dateString))
                    {
                        return DateTimeOffset.Parse(dateString, CultureInfo.InvariantCulture);
                    }
                }

                return DateTimeOffset.MinValue;
            }
        }

        public bool? Draft
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.ContainsKey("draft"))
                {
                    return (bool?)frontMatter["draft"];
                }

                return null;
            }
        }

        public string Title
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.ContainsKey("title"))
                {
                    return (string)frontMatter["title"];
                }

                return null;
            }
        }

        public List<string> Categories
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.ContainsKey("category"))
                {
                    var categories = (Tomlyn.Model.TomlArray)frontMatter["category"];
                    return categories.Select(i => (string)i).ToList();
                }

                return new List<string>();
            }
        }

        public List<string> Games
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.ContainsKey("game"))
                {
                    var games = (Tomlyn.Model.TomlArray)frontMatter["game"];
                    return games.Select(i => (string)i).ToList();
                }

                return null;
            }
        }

        public List<string> Platforms
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.ContainsKey("platform"))
                {
                    var platforms = (Tomlyn.Model.TomlArray)frontMatter["platform"];
                    return platforms.Select(i => (string)i).ToList();
                }

                return null;
            }
        }

        public List<string> Ratings
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.ContainsKey("rating"))
                {
                    var ratings = (Tomlyn.Model.TomlArray)frontMatter["rating"];
                    return ratings.Select(i => (string)i).ToList();
                }

                return null;
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
                return frontMatterBlock.Model;
            }

            return null;
        }

        public static PostData MarkdownFromFilePath(MarkdownPipeline mdPipeline, string filePath)
        {
            var text = File.ReadAllText(filePath);

            var post = new PostData();
            post.SourceFilePath = filePath;

            post.MdDoc = Markdown.Parse(text, mdPipeline);

            return post;
        }
    }
}
