using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using GlogGenerator.RenderState;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlogGenerator.HugoCompat
{
    public class Shortcodes
    {
        private static readonly Regex ShortcodeStartPattern = new Regex(@"{{[%<]\s*(?<Code>\w+)\s*(?<ArgsString>.*?)\s*[%>]}}", RegexOptions.Compiled);

        private static readonly Regex NamedArgSingleQuotePattern = new Regex(@"(?<ArgName>\w+?)\s*=\s*'(?<ArgValue>.+?)(?<!\\)'", RegexOptions.Compiled);
        private static readonly Regex NamedArgDoubleQuotePattern = new Regex(@"(?<ArgName>\w+?)\s*=\s*""(?<ArgValue>.+?)(?<!\\)""", RegexOptions.Compiled);

        private static Dictionary<string, string> EnsureIncludesNamedArgs(string argsString, params string[] requiredArgNames)
        {
            var namedArgMatches = NamedArgSingleQuotePattern.Matches(argsString).ToList();
            namedArgMatches.AddRange(NamedArgDoubleQuotePattern.Matches(argsString).ToList());

            var namedArgs = new Dictionary<string, string>();
            foreach (var namedArgMatch in namedArgMatches)
            {
                var argName = namedArgMatch.Groups["ArgName"].ToString();
                var argValue = namedArgMatch.Groups["ArgValue"].ToString();
                namedArgs.Add(argName, argValue);
            }

            foreach (var requiredArgName in requiredArgNames)
            {
                if (!namedArgs.ContainsKey(requiredArgName))
                {
                    throw new ArgumentException($"Missing required argument {requiredArgName}");
                }
            }

            return namedArgs;
        }

        public static string TranslateToHtml(FilePathResolver filePathResolver, SiteState site, PageState page, string text)
        {
            var shortcodeStartMatch = ShortcodeStartPattern.Match(text);
            while (shortcodeStartMatch.Success)
            {
                var shortcode = shortcodeStartMatch.Groups["Code"].ToString();
                var argsString = shortcodeStartMatch.Groups["ArgsString"].ToString();

                var shortcodeStartPos = shortcodeStartMatch.Index;
                var shortcodeLength = shortcodeStartMatch.Length;
                var shortcodeRequiresClose = false;
                var defaultArg = string.Empty;
                var namedArgs = new Dictionary<string, string>();
                var innerText = string.Empty;

                // Examine shortcode to determine if args are missing and if close is required.
                try
                {
                    switch (shortcode.ToLowerInvariant())
                    {
                        case "absimg":
                        case "absvideo":
                            namedArgs = EnsureIncludesNamedArgs(argsString, "src");
                            break;

                        case "abslink":
                            shortcodeRequiresClose = true;
                            namedArgs = EnsureIncludesNamedArgs(argsString, "href");
                            break;

                        default:
                            throw new ArgumentException($"Unknown shortcode {shortcode}");
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Shortcode {shortcode} at {shortcodeStartPos} missing required arguments", ex);
                }

                if (shortcodeRequiresClose)
                {
                    var shortcodeClosePattern = new Regex(string.Concat(@"{{[%<]\s*/", shortcode, @"\s*[%>]}}"), RegexOptions.Compiled);
                    var shortcodeCloseSearchStart = shortcodeStartPos + shortcodeLength;
                    var shortcodeCloseMatch = shortcodeClosePattern.Match(text, shortcodeCloseSearchStart);

                    if (!shortcodeCloseMatch.Success)
                    {
                        throw new ArgumentException($"Shortcode {shortcode} at {shortcodeStartPos} didn't find a subsequent closing code");
                    }

                    var innerTextStart = shortcodeStartPos + shortcodeLength;
                    innerText = text.Substring(innerTextStart, shortcodeCloseMatch.Index - innerTextStart);

                    shortcodeLength = (shortcodeCloseMatch.Index + shortcodeCloseMatch.Length) - shortcodeStartPos;
                }

                // Determine the shortcode's replacement text
                var replacementText = string.Empty;
                switch (shortcode.ToLowerInvariant())
                {
                    case "absimg":
                        var absimgParamsBuilder = new StringBuilder();
                        if (namedArgs.TryGetValue("alt", out var absimgAltArg))
                        {
                            var escapedAltText = HttpUtility.HtmlEncode(absimgAltArg);
                            absimgParamsBuilder.Append(CultureInfo.InvariantCulture, $" alt=\"{escapedAltText}\"");
                        }
                        if (namedArgs.TryGetValue("width", out var absimgWidthArg))
                        {
                            absimgParamsBuilder.Append(CultureInfo.InvariantCulture, $" width=\"{absimgWidthArg}\"");
                        }
                        if (namedArgs.TryGetValue("height", out var absimgHeightArg))
                        {
                            absimgParamsBuilder.Append(CultureInfo.InvariantCulture, $" height=\"{absimgHeightArg}\"");
                        }

                        var absimgUrl = $"{site.BaseURL}{namedArgs["src"]}";
                        replacementText = $"<a href=\"{absimgUrl}\"><img src=\"{absimgUrl}\"{absimgParamsBuilder.ToString()} /></a>";
                        break;

                    case "abslink":
                        var abslinkHref = namedArgs["href"];

                        // Verify that the href target is a valid site route/permalink.
                        var abslinkContentRoute = abslinkHref;
                        if (abslinkContentRoute.EndsWith('/'))
                        {
                            abslinkContentRoute += "index.html";
                        }
                        if (!site.ContentRoutes.ContainsKey(abslinkContentRoute))
                        {
                            throw new InvalidDataException($"No content route matches the abslink href \"{abslinkHref}\" (guessed route \"{abslinkContentRoute}\")");
                        }

                        var abslinkUrl = $"{site.BaseURL}{abslinkHref}";
                        replacementText = $"<a href=\"{abslinkUrl}\">{innerText}</a>";
                        break;

                    case "absvideo":
                        var absvideoParamsBuilder = new StringBuilder();
                        if (namedArgs.TryGetValue("width", out var absvideoWidthArg))
                        {
                            absvideoParamsBuilder.Append(CultureInfo.InvariantCulture, $" width=\"{absvideoWidthArg}\"");
                        }
                        if (namedArgs.TryGetValue("height", out var absvideoHeightArg))
                        {
                            absvideoParamsBuilder.Append(CultureInfo.InvariantCulture, $" height=\"{absvideoHeightArg}\"");
                        }
                        absvideoParamsBuilder.Append(" controls playsinline ");
                        if (namedArgs.TryGetValue("alt", out var absvideoAltArg))
                        {
                            var escapedAltText = HttpUtility.HtmlEncode(absvideoAltArg);
                            absvideoParamsBuilder.Append(CultureInfo.InvariantCulture, $" alt=\"{escapedAltText}\"");
                        }
                        if (namedArgs.ContainsKey("autoplay"))
                        {
                            absvideoParamsBuilder.Append(" autoplay muted loop");
                        }

                        var absvideoUrl = $"{site.BaseURL}{namedArgs["src"]}";
                        replacementText = $"<a href=\"{absvideoUrl}\"><video{absvideoParamsBuilder.ToString()}><source src=\"{absvideoUrl}\" /></video></a>";
                        break;

                    default:
                        throw new InvalidDataException($"Failed to process shortcode {shortcode} at {shortcodeStartPos} after parsing it!");
                }

                text = text.Remove(shortcodeStartPos, shortcodeLength).Insert(shortcodeStartPos, replacementText);
                shortcodeStartMatch = ShortcodeStartPattern.Match(text);
            }

            return text;
        }
    }
}
