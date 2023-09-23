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

        public static string GetPermalinkForCategoryName(SiteState site, string categoryName)
        {
            // TODO: validate the name!
            var referenceUrlized = StringRenderer.Urlize(categoryName);
            var referenceLink = $"{site.BaseURL}category/{referenceUrlized}";

            return referenceLink;
        }

        public static string GetPermalinkForGameName(SiteState site, string gameName)
        {
            _ = site.ValidateMatchingGameName(gameName);

            // TODO: grab this from the validation step!
            var referenceUrlized = StringRenderer.Urlize(gameName);
            var referenceLink = $"{site.BaseURL}game/{referenceUrlized}";

            return referenceLink;
        }

        public static string GetPermalinkForPlatformName(SiteState site, string platformName)
        {
            // TODO: validate the name!
            var referenceUrlized = StringRenderer.Urlize(platformName);
            var referenceLink = $"{site.BaseURL}platform/{referenceUrlized}";

            return referenceLink;
        }

        public static string GetPermalinkForRatingName(SiteState site, string ratingName)
        {
            // TODO: validate the name!
            var referenceUrlized = StringRenderer.Urlize(ratingName);
            var referenceLink = $"{site.BaseURL}rating/{referenceUrlized}";

            return referenceLink;
        }

        public static string GetPermalinkForTagName(SiteState site, string tagName)
        {
            _ = site.ValidateMatchingTagName(tagName);

            // TODO: grab this from the validation step!
            var referenceUrlized = StringRenderer.Urlize(tagName);
            var referenceLink = $"{site.BaseURL}tag/{referenceUrlized}";

            return referenceLink;
        }
    }
}
