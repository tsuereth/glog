using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using GlogGenerator.MarkdownExtensions;
using Markdig;

namespace GlogGenerator.Data
{
    public class PostData : ContentWithFrontMatterData
    {
        public static readonly string PostContentBaseDir = "content/post";

        [IgnoreDataMember]
        public string SourceFilePath { get; private set; } = string.Empty;

        [IgnoreDataMember]
        public string PermalinkRelative { get; private set; } = string.Empty;

        [DataMember(Name = "draft")]
        public bool Draft { get; private set; } = false;

        // Parsing hack alert!
        // Tomlyn's ToModel conversion uses Convert.ChangeType() which requires
        // the target type to implement IConvertible; but DateTimeOffset doesn't.
        // So instead of attempting to implement custom DateTimeOffset converters...
        // we'll just keep the raw data as a string, and Parse() it later.
        [DataMember(Name = "date")]
        public string DateString { get; private set; } = string.Empty;

        [IgnoreDataMember]
        public DateTimeOffset Date { get; private set; } = DateTimeOffset.MinValue;

        [DataMember(Name = "title")]
        public string Title { get; private set; } = string.Empty;

        [DataMember(Name = "category")]
        public List<string> Categories { get; private set; } = new List<string>();

        [DataMember(Name = "game")]
        public List<string> Games { get; private set; } = new List<string>();

        [DataMember(Name = "platform")]
        public List<string> Platforms { get; private set; } = new List<string>();

        [DataMember(Name = "rating")]
        public List<string> Ratings { get; private set; } = new List<string>();

        [DataMember(Name = "slug")]
        public string Slug { get; private set; } = string.Empty;

        public static PostData MarkdownFromFilePath(MarkdownPipeline mdPipeline, string filePath)
        {
            var post = ContentWithFrontMatterData.FromFilePath<PostData>(mdPipeline, filePath);
            post.SourceFilePath = filePath;
            post.Date = DateTimeOffset.Parse(post.DateString, CultureInfo.InvariantCulture);

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
                permalinkPathParts.Add(UrlizedString.Urlize(post.Title));
            }

            var permalinkPath = string.Join('/', permalinkPathParts) + '/';

            post.PermalinkRelative = permalinkPath;

            return post;
        }
    }
}
