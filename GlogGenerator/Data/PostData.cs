using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GlogGenerator.HugoCompat;

namespace GlogGenerator.Data
{
    public class PostData
    {
        public static readonly string PostContentBaseDir = "content/post";

        public string SourceFilePath { get; private set; } = string.Empty;

        public string PermalinkRelative { get; private set; } = string.Empty;

        public bool Draft { get; private set; }

        public DateTimeOffset Date { get; private set; } = DateTimeOffset.MinValue;

        public string Title { get; private set; } = string.Empty;

        public List<string> Categories { get; private set; } = new List<string>();

        public List<string> Games { get; private set; } = new List<string>();

        public List<string> Platforms { get; private set; } = new List<string>();

        public List<string> Ratings { get; private set; } = new List<string>();

        public string Slug { get; private set; } = string.Empty;

        public string Content { get; private set; } = string.Empty;

        public static PostData FromFilePath(string filePath)
        {
            var postLines = File.ReadAllLines(filePath);
            var data = FrontMatterToml.FromLines(postLines);

            var post = new PostData();
            post.SourceFilePath = filePath;

            var dateString = data.GetValue<string>("date");
            if (dateString != null)
            {
                post.Date = DateTimeOffset.Parse(dateString, CultureInfo.InvariantCulture);
            }

            post.Draft = data.GetValue<bool>("draft");
            post.Title = data.GetValue<string>("title") ?? string.Empty;
            post.Categories = data.GetValue<List<string>>("category") ?? new List<string>();
            post.Games = data.GetValue<List<string>>("game") ?? new List<string>();
            post.Platforms = data.GetValue<List<string>>("platform") ?? new List<string>();
            post.Ratings = data.GetValue<List<string>>("rating") ?? new List<string>();
            post.Slug = data.GetValue<string>("slug") ?? string.Empty;

            post.Content = data.GetText();

            var permalinkPathParts = new List<string>(4)
            {
                post.Date.Year.ToString("D4", CultureInfo.InvariantCulture),
                post.Date.Month.ToString("D2", CultureInfo.InvariantCulture),
                post.Date.Day.ToString("D2", CultureInfo.InvariantCulture),
            };

            if (!string.IsNullOrEmpty(post.Slug))
            {
                permalinkPathParts.Add(post.Slug);
            }
            else
            {
                permalinkPathParts.Add(TemplateFunctionsStringRenderer.Urlize(post.Title, htmlEncode: false));
            }

            var permalinkPath = string.Join('/', permalinkPathParts) + '/';

            post.PermalinkRelative = permalinkPath;

            return post;
        }
    }
}
