using System;
using System.Collections.Generic;
using System.Linq;
using GlogGenerator.Data;

namespace GlogGenerator.RenderState
{
    public class LinkedPostProperties
    {
        public List<string> Categories { get; set; } = new List<string>();

        public DateTimeOffset Date { get; set; } = DateTimeOffset.MinValue;

        public List<string> Games { get; set; } = new List<string>();

        public List<string> Platforms { get; set; } = new List<string>();

        public string PermalinkRelative { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public static LinkedPostProperties FromPostData(PostData postData, ISiteDataIndex siteDataIndex)
        {
            var linkedPostProperties = new LinkedPostProperties();

            linkedPostProperties.Categories = postData.Categories.Select(r => siteDataIndex.GetData(r))
                .Select(c => c.Name).ToList();

            linkedPostProperties.Date = postData.Date;

            linkedPostProperties.Games = postData.Games.Select(r => siteDataIndex.GetData(r))
                .Select(c => c.Title).ToList();

            linkedPostProperties.Platforms = postData.Platforms.Select(r => siteDataIndex.GetData(r))
                .Select(c => c.Abbreviation).ToList();

            linkedPostProperties.PermalinkRelative = postData.PermalinkRelative;

            linkedPostProperties.Title = postData.Title;

            return linkedPostProperties;
        }
    }
}
