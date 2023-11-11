using System;
using System.Collections.Generic;
using GlogGenerator.Data;
using GlogGenerator.RenderState;

namespace GlogGenerator.MarkdownExtensions
{
    public static class GlogLinkHandlers
    {
        public static readonly Dictionary<string, Func<ISiteDataIndex, SiteState, string, string>> LinkMatchHandlers = new Dictionary<string, Func<ISiteDataIndex, SiteState, string, string>>()
        {
            { "category", GetPermalinkForCategoryName },
            { "game", GetPermalinkForGameName },
            { "platform", GetPermalinkForPlatformAbbreviation },
            { "rating", GetPermalinkForRatingName },
            { "tag", GetPermalinkForTagName },
        };

        private static bool ValidateSiteHasContentForLink(SiteState site, string linkType, string linkNameUrlized)
        {
            var linkRelativePath = $"{linkType}/{linkNameUrlized}/index.html";

            if (!site.ContentRoutes.ContainsKey(linkRelativePath))
            {
                throw new ArgumentException($"No content route appears to exist at {linkRelativePath}");
            }

            return true;
        }

        public static string GetPermalinkForCategoryName(ISiteDataIndex siteDataIndex, SiteState site, string categoryName)
        {
            var referenceUrlized = UrlizedString.Urlize(categoryName);
            _ = ValidateSiteHasContentForLink(site, "category", referenceUrlized);

            return $"{site.BaseURL}category/{referenceUrlized}";
        }

        public static string GetPermalinkForGameName(ISiteDataIndex siteDataIndex, SiteState site, string gameName)
        {
            _ = siteDataIndex.GetGame(gameName);

            var referenceUrlized = UrlizedString.Urlize(gameName);
            _ = ValidateSiteHasContentForLink(site, "game", referenceUrlized);

            return $"{site.BaseURL}game/{referenceUrlized}";
        }

        public static string GetPermalinkForPlatformAbbreviation(ISiteDataIndex siteDataIndex, SiteState site, string platformAbbreviation)
        {
            _ = siteDataIndex.GetPlatform(platformAbbreviation);

            var referenceUrlized = UrlizedString.Urlize(platformAbbreviation);
            _ = ValidateSiteHasContentForLink(site, "platform", referenceUrlized);

            return $"{site.BaseURL}platform/{referenceUrlized}";
        }

        public static string GetPermalinkForRatingName(ISiteDataIndex siteDataIndex, SiteState site, string ratingName)
        {
            var referenceUrlized = UrlizedString.Urlize(ratingName);
            _ = ValidateSiteHasContentForLink(site, "rating", referenceUrlized);

            return $"{site.BaseURL}rating/{referenceUrlized}";
        }

        public static string GetPermalinkForTagName(ISiteDataIndex siteDataIndex, SiteState site, string tagName)
        {
            _ = siteDataIndex.GetTag(tagName);

            var referenceUrlized = UrlizedString.Urlize(tagName);
            _ = ValidateSiteHasContentForLink(site, "tag", referenceUrlized);

            return $"{site.BaseURL}tag/{referenceUrlized}";
        }
    }
}
