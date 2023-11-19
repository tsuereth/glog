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

        public List<SiteDataReference<CategoryData>> Categories { get { return this.categories.Select(t => t.Item2).ToList(); } }

        public List<SiteDataReference<GameData>> Games { get { return this.games.Select(t => t.Item2).ToList(); } }

        public List<SiteDataReference<PlatformData>> Platforms { get { return this.platforms.Select(t => t.Item2).ToList(); } }

        public List<SiteDataReference<RatingData>> Ratings { get { return this.ratings.Select(t => t.Item2).ToList(); } }

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

        public string ToMarkdownString(MarkdownPipeline mdPipeline, ISiteDataIndex siteDataIndex)
        {
            // Rewrite all referenceable data keys in the TOML front matter,
            // just in case those reference keys have changed since we read them.

            foreach (var frontMatterReference in this.categories)
            {
                var tomlString = frontMatterReference.Item1;
                var data = siteDataIndex.GetData(frontMatterReference.Item2);
                tomlString.Value = data.GetReferenceableKey();
            }

            foreach (var frontMatterReference in this.games)
            {
                var tomlString = frontMatterReference.Item1;
                var data = siteDataIndex.GetData(frontMatterReference.Item2);
                tomlString.Value = data.GetReferenceableKey();
            }

            foreach (var frontMatterReference in this.platforms)
            {
                var tomlString = frontMatterReference.Item1;
                var data = siteDataIndex.GetData(frontMatterReference.Item2);
                tomlString.Value = data.GetReferenceableKey();
            }

            foreach (var frontMatterReference in this.ratings)
            {
                var tomlString = frontMatterReference.Item1;
                var data = siteDataIndex.GetData(frontMatterReference.Item2);
                tomlString.Value = data.GetReferenceableKey();
            }

            return this.MdDoc.ToMarkdownString(mdPipeline);
        }

        public void RewriteSourceFile(MarkdownPipeline mdPipeline, ISiteDataIndex siteDataIndex)
        {
            if (string.IsNullOrEmpty(SourceFilePath))
            {
                throw new InvalidDataException("SourceFilePath is empty");
            }

            var fileContent = this.ToMarkdownString(mdPipeline, siteDataIndex);

            var utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            File.WriteAllText(this.SourceFilePath, fileContent, utf8WithoutBom);
        }

        public MarkdownDocument MdDoc { get; private set; }

        private DateTimeOffset date = DateTimeOffset.MinValue;
        private bool? draft = null;
        private string title = null;
        private string slug = null;
        private List<Tuple<Tommy.TomlString, SiteDataReference<CategoryData>>> categories = new List<Tuple<Tommy.TomlString, SiteDataReference<CategoryData>>>();
        private List<Tuple<Tommy.TomlString, SiteDataReference<GameData>>> games = new List<Tuple<Tommy.TomlString, SiteDataReference<GameData>>>();
        private List<Tuple<Tommy.TomlString, SiteDataReference<PlatformData>>> platforms = new List<Tuple<Tommy.TomlString, SiteDataReference<PlatformData>>>();
        private List<Tuple<Tommy.TomlString, SiteDataReference<RatingData>>> ratings = new List<Tuple<Tommy.TomlString, SiteDataReference<RatingData>>>();

        public static PostData MarkdownFromFilePath(MarkdownPipeline mdPipeline, string filePath, ISiteDataIndex siteDataIndex)
        {
            var fileContent = File.ReadAllText(filePath);
            var post = MarkdownFromString(mdPipeline, fileContent, siteDataIndex);
            post.SourceFilePath = filePath;

            return post;
        }

        public static PostData MarkdownFromString(MarkdownPipeline mdPipeline, string fileContent, ISiteDataIndex siteDataIndex)
        {
            var post = new PostData();

            post.MdDoc = Markdown.Parse(fileContent, mdPipeline);

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
                        var categoryReference = siteDataIndex.CreateReference<CategoryData>(categoryName.ToString());

                        var frontMatterReference = Tuple.Create(categoryName as Tommy.TomlString, categoryReference);
                        post.categories.Add(frontMatterReference);
                    }
                }

                if (frontMatter.TryGetNode("game", out var frontMatterGames))
                {
                    foreach (var gameTitle in frontMatterGames)
                    {
                        var gameReference = siteDataIndex.CreateReference<GameData>(gameTitle.ToString());

                        var frontMatterReference = Tuple.Create(gameTitle as Tommy.TomlString, gameReference);
                        post.games.Add(frontMatterReference);
                    }
                }

                if (frontMatter.TryGetNode("platform", out var frontMatterPlatforms))
                {
                    foreach (var platformAbbreviation in frontMatterPlatforms)
                    {
                        var platformReference = siteDataIndex.CreateReference<PlatformData>(platformAbbreviation.ToString());

                        var frontMatterReference = Tuple.Create(platformAbbreviation as Tommy.TomlString, platformReference);
                        post.platforms.Add(frontMatterReference);
                    }
                }

                if (frontMatter.TryGetNode("rating", out var frontMatterRatings))
                {
                    foreach (var ratingName in frontMatterRatings)
                    {
                        var ratingReference = siteDataIndex.CreateReference<RatingData>(ratingName.ToString());

                        var frontMatterReference = Tuple.Create(ratingName as Tommy.TomlString, ratingReference);
                        post.ratings.Add(frontMatterReference);
                    }
                }
            }

            return post;
        }
    }
}
