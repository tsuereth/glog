using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GlogGenerator.Data;
using GlogGenerator.HugoCompat;
using GlogGenerator.RenderState;

namespace GlogGenerator
{
    public static class BuildStaticSite
    {
        public static void Build(
            string inputFilesBasePath,
            string templateFilesBasePath,
            string outputBasePath)
        {
            var configFilePath = Path.Combine(inputFilesBasePath, "config.toml");
            var config = ConfigData.FromFilePath(configFilePath);

            config.BaseURL = "https://tsuereth.com/glog/";

            var site = SiteState.FromConfigData(config);
            site.OutputBasePath = outputBasePath;

            // Copy static content.
            var staticBasePath = Path.Combine(site.PathResolver.BasePath, "static");
            var staticFilePaths = Directory.EnumerateFiles(staticBasePath, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var staticFilePath in staticFilePaths)
            {
                var outputRelativePath = staticFilePath.Replace(staticBasePath + Path.DirectorySeparatorChar, string.Empty);
                var outputPath = Path.Combine(site.OutputBasePath, outputRelativePath);

                var outputDir = Path.GetDirectoryName(outputPath);
                if (string.IsNullOrEmpty(outputDir))
                {
                    throw new InvalidDataException($"Static file output path {outputPath} has empty dirname");
                }

                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir + Path.DirectorySeparatorChar);
                }

                File.Copy(staticFilePath, outputPath, overwrite: true);
            }

            // StringTemplate requires an absolute filepath.
            if (!Path.IsPathRooted(templateFilesBasePath))
            {
                templateFilesBasePath = Path.GetFullPath(templateFilesBasePath);
            }

            var templateGroup = new Antlr4.StringTemplate.TemplateGroupDirectory(
                templateFilesBasePath,
                delimiterStartChar: '%',
                delimiterStopChar: '%');

            templateGroup.RegisterRenderer(typeof(DateTimeOffset), new TemplateFunctionsDateTimeRenderer());
            templateGroup.RegisterRenderer(typeof(string), new TemplateFunctionsStringRenderer());

            // Parse content to collect data.
            var gameContentBasePath = Path.Combine(inputFilesBasePath, GameData.GameContentBaseDir);
            var gamePaths = Directory.EnumerateFiles(gameContentBasePath, "_index.md", SearchOption.AllDirectories).ToList();

            var postContentBasePath = Path.Combine(inputFilesBasePath, PostData.PostContentBaseDir);
            var postPaths = Directory.EnumerateFiles(postContentBasePath, "*.md", SearchOption.AllDirectories).ToList();

            foreach (var gamePath in gamePaths)
            {
                var gameData = GameData.FromFilePath(gamePath);
                var gameKey = TemplateFunctionsStringRenderer.Urlize(gameData.Title, htmlEncode: false);
                site.Games[gameKey] = gameData;

                foreach (var tag in gameData.Tags)
                {
                    site.AddTagIfMissing(tag);
                }
            }

            var allPosts = new List<PostData>();
            foreach (var postPath in postPaths)
            {
                var postData = PostData.FromFilePath(postPath);

                if (postData.Draft)
                {
                    continue;
                }

                allPosts.Add(postData);

                foreach (var category in postData.Categories)
                {
                    var categoryData = site.AddCategoryIfMissing(category, overwriteData: true);
                    categoryData.LinkedPosts.Add(postData);
                }

                var gameTagsByUrlized = new Dictionary<string, TagData>();
                foreach (var game in postData.Games)
                {
                    // Quirk note: a "better" source of GameData has already been processed
                    // (from game files directly), so don't allow post metadata to overwrite that.
                    var gameData = site.AddGameIfMissing(game, overwriteData: false);
                    gameData.LinkedPosts.Add(postData);

                    foreach (var tag in gameData.Tags)
                    {
                        var tagUrlized = TemplateFunctionsStringRenderer.Urlize(tag, htmlEncode: false);
                        var tagData = site.AddTagIfMissing(tag, overwriteData: false);
                        gameTagsByUrlized[tagUrlized] = tagData;
                    }
                }

                foreach (var tagData in gameTagsByUrlized.Values)
                {
                    tagData.LinkedPosts.Add(postData);
                }

                foreach (var platform in postData.Platforms)
                {
                    var platformData = site.AddPlatformIfMissing(platform, overwriteData: true);
                    platformData.LinkedPosts.Add(postData);
                }

                foreach (var rating in postData.Ratings)
                {
                    var ratingData = site.AddRatingIfMissing(rating, overwriteData: true);
                    ratingData.LinkedPosts.Add(postData);
                }

                // We need to run shortcode parsing to detect all games, tags, etc.
                _ = PageState.FromPostData(site, postData);
            }

            allPosts = allPosts.OrderByDescending(p => p.Date).ToList();

            // Now, we can render content.
            var allPostPages = new List<PageState>(allPosts.Count);
            foreach (var postData in allPosts)
            {
                var page = PageState.FromPostData(site, postData);
                allPostPages.Add(page);
                page.RenderAndWriteFile(site, templateGroup.GetInstanceOf("single"), page.OutputHtmlPath);
            }

            var postsListPage = new PageState()
            {
                HideDate = true,
                Title = "Posts",
                PageType = "posts",
                Permalink = $"{site.BaseURL}post/",
                OutputHtmlPath = Path.Combine(site.OutputBasePath, "post", "index.html"),
                LinkedPosts = allPosts,
            };
            postsListPage.RenderAndWriteFile(site, templateGroup.GetInstanceOf("list"), postsListPage.OutputHtmlPath);

            var historyPage = new PageState()
            {
                HideDate = true,
                HideTitle = true,
                Permalink = site.BaseURL, // BUG?: every history page has the BaseURL permalink!
            };
            const int pagesPerHistoryPage = 10;
            for (var historyPageNum = 0; (historyPageNum * pagesPerHistoryPage) < allPostPages.Count; ++historyPageNum)
            {
                var pageOneBased = historyPageNum + 1;

                var firstPostNum = historyPageNum * pagesPerHistoryPage;
                var postsCount = pagesPerHistoryPage;
                if (firstPostNum + postsCount >= allPostPages.Count)
                {
                    postsCount = allPostPages.Count - firstPostNum;
                }

                historyPage.HistoryPosts = allPostPages.GetRange(firstPostNum, postsCount);

                if (historyPageNum == 0)
                {
                    historyPage.HidePrevLink = true;
                }
                else if (historyPageNum == 1)
                {
                    historyPage.HidePrevLink = false;
                    historyPage.PrevLinkRelative = string.Empty; // special case! link to BaseURL.
                }
                else
                {
                    historyPage.HidePrevLink = false;
                    var prevPageOneBased = pageOneBased - 1;
                    historyPage.PrevLinkRelative = $"page/{prevPageOneBased}/";
                }

                if (((historyPageNum + 1) * pagesPerHistoryPage) >= allPostPages.Count)
                {
                    historyPage.HideNextLink = true;
                }
                else
                {
                    historyPage.HideNextLink = false;
                    var nextPageOneBased = pageOneBased + 1;
                    historyPage.NextLinkRelative = $"page/{nextPageOneBased}/";
                }

                if (historyPageNum == 0)
                {
                    historyPage.OutputHtmlPath = Path.Combine(site.OutputBasePath, "index.html");
                }
                else
                {
                    historyPage.OutputHtmlPath = Path.Combine(site.OutputBasePath, "page", pageOneBased.ToString(CultureInfo.InvariantCulture),
                        "index.html");
                }

                historyPage.RenderAndWriteFile(site, templateGroup.GetInstanceOf("history"), historyPage.OutputHtmlPath);
            }

            var rssFeedPage = new PageState()
            {
                Date = allPosts[0].Date,
                Permalink = site.BaseURL,
                HistoryPosts = allPostPages.GetRange(0, 15),
            };
            rssFeedPage.RenderAndWriteFile(site, templateGroup.GetInstanceOf("rss"), Path.Combine(site.OutputBasePath, "index.xml"));

            var categoriesIndex = new PageState()
            {
                HideDate = true,
                OutputHtmlPath = Path.Combine(site.OutputBasePath, "category", "index.html"),
                Permalink = $"{site.BaseURL}category/",
                Title = "Categories",
                PageType = "categories",
                Terms = site.CategoriesSorted,
                TermsType = "category",
            };
            categoriesIndex.RenderAndWriteFile(site, templateGroup.GetInstanceOf("termslist"), categoriesIndex.OutputHtmlPath);

            foreach (var categoryData in site.Categories.Values)
            {
                var page = PageState.FromCategoryData(site, categoryData);
                page.RenderAndWriteFile(site, templateGroup.GetInstanceOf("list"), page.OutputHtmlPath);
            }

            var gamesIndex = new PageState()
            {
                HideDate = true,
                OutputHtmlPath = Path.Combine(site.OutputBasePath, "game", "index.html"),
                Permalink = $"{site.BaseURL}game/",
                Title = "Games",
                PageType = "games",
                Terms = site.Games.Values.Select(g => g.Title).OrderBy(t => t, StringComparer.Ordinal).ToList(),
                TermsType = "game",
            };
            gamesIndex.RenderAndWriteFile(site, templateGroup.GetInstanceOf("termslist"), gamesIndex.OutputHtmlPath);

            foreach (var gameData in site.Games.Values)
            {
                var page = PageState.FromGameData(site, gameData);
                page.RenderAndWriteFile(site, templateGroup.GetInstanceOf("list_game"), page.OutputHtmlPath);
            }

            var platformsIndex = new PageState()
            {
                HideDate = true,
                OutputHtmlPath = Path.Combine(site.OutputBasePath, "platform", "index.html"),
                Permalink = $"{site.BaseURL}platform/",
                Title = "Platforms",
                PageType = "platforms",
                Terms = site.Platforms.Values.Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToList(),
                TermsType = "platform",
            };
            platformsIndex.RenderAndWriteFile(site, templateGroup.GetInstanceOf("termslist"), platformsIndex.OutputHtmlPath);

            foreach (var platformData in site.Platforms.Values)
            {
                var page = PageState.FromPlatformData(site, platformData);
                page.RenderAndWriteFile(site, templateGroup.GetInstanceOf("list"), page.OutputHtmlPath);
            }

            var ratingsIndex = new PageState()
            {
                HideDate = true,
                OutputHtmlPath = Path.Combine(site.OutputBasePath, "rating", "index.html"),
                Permalink = $"{site.BaseURL}rating/",
                Title = "Ratings",
                PageType = "ratings",
                Terms = site.Ratings.Values.Select(r => r.Name).OrderBy(n => n).ToList(),
                TermsType = "rating",
            };
            ratingsIndex.RenderAndWriteFile(site, templateGroup.GetInstanceOf("termslist"), ratingsIndex.OutputHtmlPath);

            foreach (var ratingData in site.Ratings.Values)
            {
                var page = PageState.FromRatingData(site, ratingData);
                page.RenderAndWriteFile(site, templateGroup.GetInstanceOf("list"), page.OutputHtmlPath);
            }

            var tagsIndex = new PageState()
            {
                HideDate = true,
                OutputHtmlPath = Path.Combine(site.OutputBasePath, "tag", "index.html"),
                Permalink = $"{site.BaseURL}tag/",
                Title = "Tags",
                PageType = "tags",
                Terms = site.Tags.Values.Select(t => t.Name).OrderBy(n => n, StringComparer.Ordinal).ToList(),
                TermsType = "tag",
            };
            tagsIndex.RenderAndWriteFile(site, templateGroup.GetInstanceOf("termslist"), tagsIndex.OutputHtmlPath);

            foreach (var tagData in site.Tags.Values)
            {
                var page = PageState.FromTagData(site, tagData);
                page.RenderAndWriteFile(site, templateGroup.GetInstanceOf("list_tag"), page.OutputHtmlPath);
            }

            var additionalPageFilePaths = new List<string>()
            {
                Path.Combine(config.DataBasePath, "content", "backlog.md"),
                Path.Combine(config.DataBasePath, "content", "upcoming.md"),
            };
            foreach (var pageFilePath in additionalPageFilePaths)
            {
                var pageData = PageData.FromFilePath(pageFilePath);
                var page = PageState.FromPageData(site, pageData);
                page.RenderAndWriteFile(site, templateGroup.GetInstanceOf("single"), page.OutputHtmlPath);
            }
        }
    }
}
