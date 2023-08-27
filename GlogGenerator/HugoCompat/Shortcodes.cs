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

        private static string EnsureNonEmptyDefaultArg(string argsString)
        {
            var defaultArg = argsString.Trim(new char[] { '\'', '"' });

            if (string.IsNullOrEmpty(defaultArg))
            {
                throw new ArgumentException("Missing default (unnamed) argument string");
            }

            return defaultArg;
        }

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
                        case "spoiler":
                            shortcodeRequiresClose = true;
                            break;

                        case "category":
                        case "game":
                        case "platform":
                        case "rating":
                        case "tag":
                            shortcodeRequiresClose = true;
                            defaultArg = EnsureNonEmptyDefaultArg(argsString);
                            break;

                        case "absimg":
                        case "absvideo":
                            namedArgs = EnsureIncludesNamedArgs(argsString, "src");
                            break;

                        case "abslink":
                            shortcodeRequiresClose = true;
                            namedArgs = EnsureIncludesNamedArgs(argsString, "href");
                            break;

                        case "chart":
                            namedArgs = EnsureIncludesNamedArgs(argsString, "datafile");
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
                        var abslinkUrl = $"{site.BaseURL}{namedArgs["href"]}";
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

                    case "category":
                        var categoryName = defaultArg;
                        site.AddCategoryIfMissing(categoryName);

                        replacementText = $"<a href=\"{site.BaseURL}category/{TemplateFunctionsStringRenderer.Urlize(categoryName, htmlEncode: true)}\">{innerText}</a>";
                        break;

                    case "chart":
                        var chartDatafilePath = filePathResolver.Resolve(namedArgs["datafile"]);
                        var chartDatafileContent = File.ReadAllText(chartDatafilePath);

                        // We need to escape the JSON data, to make it safe for JavaScript to load as a string.
                        var chartDatafileJson = JObject.Parse(chartDatafileContent);
                        var chartDatafileJsonString = JsonConvert.SerializeObject(chartDatafileJson, Formatting.None);
                        var chartDataString = HttpUtility.JavaScriptStringEncode(chartDatafileJsonString);

                        string chartType;
                        if (namedArgs.TryGetValue("type", out var chartTypeArg))
                        {
                            chartType = chartTypeArg;
                        }
                        else
                        {
                            chartType = "BarChart";
                        }

                        var chartOptionsPairs = namedArgs.Where(kv => !kv.Key.Equals("datafile", StringComparison.OrdinalIgnoreCase) && !kv.Key.Equals("type", StringComparison.OrdinalIgnoreCase));
                        var chartOptionsTextBuilder = new StringBuilder();
                        foreach (var chartOptionsPair in chartOptionsPairs.ToImmutableSortedDictionary())
                        {
                            var valueIsJson = chartOptionsPair.Value.StartsWith('{') || chartOptionsPair.Value.StartsWith('[');
                            var optionKey = chartOptionsPair.Key;
                            string optionValue;
                            if (valueIsJson)
                            {
                                // The raw value has escaped quotation marks, we need to un-escape them.
                                optionValue = chartOptionsPair.Value.Replace("\\\"", "\"");
                            }
                            else
                            {
                                // The raw value needs to be quoted.
                                optionValue = $"'{chartOptionsPair.Value}'";
                            }

                            chartOptionsTextBuilder.AppendLine();
                            chartOptionsTextBuilder.AppendLine();
                            chartOptionsTextBuilder.AppendLine(CultureInfo.InvariantCulture, $"\toptions['{optionKey}'] = {optionValue};");
                            chartOptionsTextBuilder.AppendLine();
                        }

                        #pragma warning disable CA5351 // Yeah MD5 is cryptographically insecure; this isn't security!
                        var pageHashInBytes = Encoding.UTF8.GetBytes(page.Permalink);
                        var pageHashOutBytes = MD5.HashData(pageHashInBytes);
                        var pageHash = Convert.ToHexString(pageHashOutBytes);

                        var chartHashInString = JsonConvert.SerializeObject(namedArgs);
                        var chartHashInBytes = Encoding.UTF8.GetBytes(chartHashInString);
                        var chartHashOutBytes = MD5.HashData(chartHashInBytes);
                        var chartHash = Convert.ToHexString(chartHashOutBytes);
                        #pragma warning restore CA5351

                        replacementText = $@"<script type=""text/javascript"">
function drawChart_{pageHash}_{chartHash}() {{
	var data = new google.visualization.DataTable(""{chartDataString}"");
	var options = {{}};
{chartOptionsTextBuilder.ToString()}
	var element = document.getElementById(""chart_{pageHash}_{chartHash}"");
	var chart = new google.visualization.{chartType}(element);
	chart.draw(data, options);
}}
</script>
<noscript><i>A Google Chart would go here, but JavaScript is disabled.</i></noscript>
<div class=""center""><chart id=""chart_{pageHash}_{chartHash}"" callback=""drawChart_{pageHash}_{chartHash}""></chart></div>";
                        break;

                    case "game":
                        var gameName = defaultArg;
                        site.AddGameIfMissing(gameName);

                        replacementText = $"<a href=\"{site.BaseURL}game/{TemplateFunctionsStringRenderer.Urlize(gameName, htmlEncode: true)}\">{innerText}</a>";
                        break;

                    case "platform":
                        var platformName = defaultArg;
                        site.AddPlatformIfMissing(platformName);

                        replacementText = $"<a href=\"{site.BaseURL}platform/{TemplateFunctionsStringRenderer.Urlize(platformName, htmlEncode: true)}\">{innerText}</a>";
                        break;

                    case "rating":
                        var ratingName = defaultArg;
                        site.AddRatingIfMissing(ratingName);

                        replacementText = $"<a href=\"{site.BaseURL}rating/{TemplateFunctionsStringRenderer.Urlize(ratingName, htmlEncode: true)}\">{innerText}</a>";
                        break;

                    case "spoiler":
                        replacementText = $"<noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">{innerText}</spoiler>";
                        break;

                    case "tag":
                        var tagName = defaultArg;
                        site.AddTagIfMissing(tagName);

                        replacementText = $"<a href=\"{site.BaseURL}tag/{TemplateFunctionsStringRenderer.Urlize(tagName, htmlEncode: true)}\">{innerText}</a>";
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
