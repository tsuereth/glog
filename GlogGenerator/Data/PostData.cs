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
                if (frontMatter != null && frontMatter.TryGetNode("slug", out var frontMatterSlug))
                {
                    var slugString = frontMatterSlug.ToString();
                    if (!string.IsNullOrEmpty(slugString))
                    {
                        specifiedSlug = true;
                        permalinkPathParts.Add(slugString);
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
                if (frontMatter != null && frontMatter.TryGetNode("date", out var frontMatterDate))
                {
                    var dateString = frontMatterDate.ToString();
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
                if (frontMatter != null && frontMatter.TryGetNode("draft", out var frontMatterDraft))
                {
                    return frontMatterDraft.AsBoolean;
                }

                return null;
            }
        }

        public string Title
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.TryGetNode("title", out var frontMatterTitle))
                {
                    return frontMatterTitle.ToString();
                }

                return null;
            }
        }

        public List<string> Categories
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.TryGetNode("category", out var frontMatterCategories))
                {
                    var categories = new List<string>();
                    foreach (var categoryName in frontMatterCategories)
                    {
                        categories.Add(categoryName.ToString());
                    }
                    return categories;
                }

                return new List<string>();
            }
        }

        public List<string> Games
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.TryGetNode("game", out var frontMatterGames))
                {
                    var games = new List<string>();
                    foreach (var gameName in frontMatterGames)
                    {
                        games.Add(gameName.ToString());
                    }
                    return games;
                }

                return null;
            }
        }

        public List<string> Platforms
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.TryGetNode("platform", out var frontMatterPlatforms))
                {
                    var platforms = new List<string>();
                    foreach (var platformName in frontMatterPlatforms)
                    {
                        platforms.Add(platformName.ToString());
                    }
                    return platforms;
                }

                return null;
            }
        }

        public List<string> Ratings
        {
            get
            {
                var frontMatter = this.GetFrontMatter();
                if (frontMatter != null && frontMatter.TryGetNode("rating", out var frontMatterRatings))
                {
                    var ratings = new List<string>();
                    foreach (var ratingName in frontMatterRatings)
                    {
                        ratings.Add(ratingName.ToString());
                    }
                    return ratings;
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

        private Tommy.TomlTable GetFrontMatter()
        {
            var frontMatterBlock = this.MdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();
            if (frontMatterBlock != null)
            {
                return frontMatterBlock.GetModel();
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
