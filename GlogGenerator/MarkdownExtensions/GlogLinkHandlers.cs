using System;
using System.Collections.Generic;
using GlogGenerator.RenderState;
using GlogGenerator.TemplateRenderers;

namespace GlogGenerator.MarkdownExtensions
{
    public static class GlogLinkHandlers
    {
        public static readonly Dictionary<string, Func<SiteState, string, string>> LinkMatchHandlers = new Dictionary<string, Func<SiteState, string, string>>()
        {
            { "category", GetPermalinkForCategoryName },
            { "game", GetPermalinkForGameName },
            { "platform", GetPermalinkForPlatformName },
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

        public static string GetPermalinkForCategoryName(SiteState site, string categoryName)
        {
            var referenceUrlized = StringRenderer.Urlize(categoryName);
            _ = ValidateSiteHasContentForLink(site, "category", referenceUrlized);

            return $"{site.BaseURL}category/{referenceUrlized}";
        }

        public static string GetPermalinkForGameName(SiteState site, string gameName)
        {
            _ = site.ValidateMatchingGameName(gameName);

            var referenceUrlized = StringRenderer.Urlize(gameName);
            _ = ValidateSiteHasContentForLink(site, "game", referenceUrlized);

            return $"{site.BaseURL}game/{referenceUrlized}";
        }

        public static string GetPermalinkForPlatformName(SiteState site, string platformName)
        {
            // TODO: Match platform references against SiteState (and its IGDB cache).

            var referenceUrlized = StringRenderer.Urlize(platformName);

            // TODO: Re-enable this content route validation once we're sure that
            // every platform reference has valid data from IGDB.
            //_ = ValidateSiteHasContentForLink(site, "platform", referenceUrlized);

            return $"{site.BaseURL}platform/{referenceUrlized}";
        }

        public static string GetPermalinkForRatingName(SiteState site, string ratingName)
        {
            var referenceUrlized = StringRenderer.Urlize(ratingName);
            _ = ValidateSiteHasContentForLink(site, "rating", referenceUrlized);

            return $"{site.BaseURL}rating/{referenceUrlized}";
        }

        public static string GetPermalinkForTagName(SiteState site, string tagName)
        {
            _ = site.ValidateMatchingTagName(tagName);

            var referenceUrlized = StringRenderer.Urlize(tagName);
            _ = ValidateSiteHasContentForLink(site, "tag", referenceUrlized);

            return $"{site.BaseURL}tag/{referenceUrlized}";
        }
    }
}
