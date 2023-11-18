using System;
using System.Collections.Generic;
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

                if (!string.IsNullOrEmpty(this.slug))
                {
                    permalinkPathParts.Add(this.slug);
                }
                else
                {
                    permalinkPathParts.Add(UrlizedString.Urlize(this.Title));
                }    

                return string.Join('/', permalinkPathParts) + '/';
            }
        }

        public DateTimeOffset Date { get { return this.date; } }

        public bool? Draft { get { return this.draft; } }

        public string Title { get { return this.title; } }

        public List<SiteDataReference<CategoryData>> Categories { get { return this.categories; } }

        public List<SiteDataReference<GameData>> Games { get { return this.games; } }

        public List<SiteDataReference<PlatformData>> Platforms { get { return this.platforms; } }

        public List<SiteDataReference<RatingData>> Ratings { get { return this.ratings; } }

        public string GetPostId()
        {
            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var typeBytes = Encoding.UTF8.GetBytes(nameof(PostData));
                hash.AppendData(typeBytes);

                var permalinkRelativeBytes = Encoding.UTF8.GetBytes(this.PermalinkRelative);
                hash.AppendData(permalinkRelativeBytes);

                var idBytes = hash.GetCurrentHash();
                return Convert.ToHexString(idBytes);
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

        private DateTimeOffset date = DateTimeOffset.MinValue;
        private bool? draft = null;
        private string title = null;
        private string slug = null;
        private List<SiteDataReference<CategoryData>> categories = new List<SiteDataReference<CategoryData>>();
        private List<SiteDataReference<GameData>> games = new List<SiteDataReference<GameData>>();
        private List<SiteDataReference<PlatformData>> platforms = new List<SiteDataReference<PlatformData>>();
        private List<SiteDataReference<RatingData>> ratings = new List<SiteDataReference<RatingData>>();

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

            var frontMatterBlock = post.MdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();
            if (frontMatterBlock != null)
            {
                var frontMatter = frontMatterBlock.GetModel();

                if (frontMatter.TryGetNode("date", out var frontMatterDate))
                {
                    var dateString = frontMatterDate.ToString();
                    if (!string.IsNullOrEmpty(dateString))
                    {
                        post.date = DateTimeOffset.Parse(dateString, CultureInfo.InvariantCulture);
                    }
                }

                if (frontMatter.TryGetNode("draft", out var frontMatterDraft))
                {
                    post.draft = frontMatterDraft.AsBoolean;
                }

                if (frontMatter.TryGetNode("title", out var frontMatterTitle))
                {
                    post.title = frontMatterTitle.ToString();
                }

                if (frontMatter.TryGetNode("slug", out var frontMatterSlug))
                {
                    post.slug = frontMatterSlug.ToString();
                }

                if (frontMatter.TryGetNode("category", out var frontMatterCategories))
                {
                    foreach (var categoryName in frontMatterCategories)
                    {
                        var categoryReference = new SiteDataReference<CategoryData>(categoryName.ToString());

                        post.categories.Add(categoryReference);
                    }
                }

                if (frontMatter.TryGetNode("game", out var frontMatterGames))
                {
                    foreach (var gameTitle in frontMatterGames)
                    {
                        var gameReference = new SiteDataReference<GameData>(gameTitle.ToString());

                        post.games.Add(gameReference);
                    }
                }

                if (frontMatter.TryGetNode("platform", out var frontMatterPlatforms))
                {
                    foreach (var platformAbbreviation in frontMatterPlatforms)
                    {
                        var platformReference = new SiteDataReference<PlatformData>(platformAbbreviation.ToString());

                        post.platforms.Add(platformReference);
                    }
                }

                if (frontMatter.TryGetNode("rating", out var frontMatterRatings))
                {
                    foreach (var ratingName in frontMatterRatings)
                    {
                        var ratingReference = new SiteDataReference<RatingData>(ratingName.ToString());

                        post.ratings.Add(ratingReference);
                    }
                }
            }

            return post;
        }
    }
}
