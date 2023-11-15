using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GlogGenerator.Data;
using GlogGenerator.IgdbApi;
using GlogGenerator.RenderState;
using GlogGenerator.Stats;
using Microsoft.Extensions.Logging;
using Mono.Options;

namespace GlogGenerator
{
    public class Program
    {
        static async Task<int> Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(c => c.AddConsole());

            var logger = loggerFactory.CreateLogger<Program>();

            var projectProperties = Assembly.GetEntryAssembly().GetCustomAttribute<ProjectPropertiesAttribute>();

            var igdbClientId = string.Empty;
            var igdbClientSecret = string.Empty;
            var inputFilesBasePath = projectProperties.DefaultInputFilesBasePath;
            var templateFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "templates");
            var hostOrigin = "http://localhost:1313";
            var newPostPath = string.Empty;
            var pathPrefix = "/glog/";
            var reportPath = string.Empty;
            var reportEndDateString = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ss", null);
            var reportStartDateString = DateTimeOffset.Now.ToString("yyyy-01-01T00:00:00", null);
            var staticSiteOutputBasePath = Path.Combine(Directory.GetCurrentDirectory(), "public");
            var updateIgdbCache = false;
            var rewriteInputFiles = false;

            var options = new OptionSet()
            {
                { "igdb-client-id=", "IGDB API (Twitch Developers) Client ID", o => igdbClientId = o },
                { "igdb-client-secret=", "IGDB API (Twitch Developers) Client Secret", o => igdbClientSecret = o },
                { "i|input-path=", $"Input files base path, default: {inputFilesBasePath}", o => inputFilesBasePath = o },
                { "t|templates-path=", $"Template files base path, default: {templateFilesBasePath}", o => templateFilesBasePath = o },
                { "h|host-origin=", $"Host origin (scheme + name + port) for the site, default: {hostOrigin}", o => hostOrigin = o },
                { "n|new-post=", $"NEW|UNDRAFT: local path to a new/draft post file", o => newPostPath = o },
                { "p|path-prefix=", $"Path prefix, default: {pathPrefix}", o => pathPrefix = o },
                { "r|report-path=", "REPORTSTATS: local path to output a report", o => reportPath = o },
                { "e|end-date=", $"REPORTSTATS: end date for generating a report, default: {reportEndDateString}", o => reportEndDateString = o },
                { "s|start-date=", $"REPORTSTATS: start date for generating a report, default: {reportStartDateString}", o => reportStartDateString = o },
                { "o|output-path=", $"BUILD: static site output base path, default: {staticSiteOutputBasePath}", o => staticSiteOutputBasePath = o },
                { "u|update-igdb-cache=", $"Update the IGDB data cache (requires IGDB API credentials), default: {updateIgdbCache}", o => updateIgdbCache = bool.Parse(o) },
                { "w|rewrite-input-files=", $"Rewrite input files after loading site data, default: {rewriteInputFiles}", o => rewriteInputFiles = bool.Parse(o) },
            };
            var verbs = options.Parse(args);

            var activeVerb = string.Empty;
            if (verbs.Count == 0)
            {
                logger.LogWarning("No verbs provided, defaulting to `host`");
                activeVerb = "host";
            }
            else
            {
                activeVerb = verbs[0];
                if (verbs.Count > 1)
                {
                    logger.LogInformation(
                        "More than one verb was provided, using `{ActiveVerb}` and ignoring the following: {IgnoredVerbs}",
                        activeVerb,
                        string.Join(", ", verbs.GetRange(1, verbs.Count - 1)));
                }
            }

            var activeVerbLowercase = activeVerb.ToLowerInvariant();

            var configFilePath = Path.Combine(inputFilesBasePath, "config.toml");
            var configData = ConfigData.FromFilePaths(configFilePath, inputFilesBasePath, templateFilesBasePath);
            var builder = new SiteBuilder(logger, configData);
            builder.SetBaseURL($"{hostOrigin}{pathPrefix}"); // TODO: ensure proper slash-usage between origin and path

            var activeVerbMustLoadSiteData = (
                activeVerbLowercase.Equals("build", StringComparison.Ordinal) ||
                activeVerbLowercase.Equals("host", StringComparison.Ordinal) ||
                activeVerbLowercase.Equals("reportstats", StringComparison.Ordinal));
            if (activeVerbMustLoadSiteData || updateIgdbCache || rewriteInputFiles)
            {
                LoadSiteData(logger, builder);
            }

            if (updateIgdbCache)
            {
                if (string.IsNullOrEmpty(igdbClientId))
                {
                    throw new ArgumentException("Missing or empty --igdb-client-id");
                }
                if (string.IsNullOrEmpty(igdbClientSecret))
                {
                    throw new ArgumentException("Missing or empty --igdb-client-secret");
                }

                using (var igdbApiClient = new IgdbApiClient(logger, igdbClientId, igdbClientSecret))
                {
                    logger.LogInformation("Updating IGDB cache...");
                    var cacheUpdateTimer = Stopwatch.StartNew();
                    await builder.UpdateIgdbCacheFromApiAsync(igdbApiClient);
                    cacheUpdateTimer.Stop();
                    logger.LogInformation(
                        "Finished updating in {CacheUpdateTimeMs} ms",
                        cacheUpdateTimer.ElapsedMilliseconds);
                }

                // Metadata may have changed, so we need to reload the index.
                LoadSiteData(logger, builder);
            }

            if (rewriteInputFiles)
            {
                logger.LogInformation("Rewriting input files...");
                var rewriteTimer = Stopwatch.StartNew();
                builder.RewriteData();
                rewriteTimer.Stop();
                logger.LogInformation(
                    "Finished rewriting input files in {RewriteTimeMs} ms",
                    rewriteTimer.ElapsedMilliseconds);
            }

            var outputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            switch (activeVerbLowercase)
            {
                case "build":
                    LoadSiteRoutes(logger, builder);

                    logger.LogInformation("Building content...");
                    var buildTimer = Stopwatch.StartNew();
                    BuildStaticSite.Build(builder.GetSiteState(), staticSiteOutputBasePath);
                    buildTimer.Stop();
                    logger.LogInformation(
                        "Finished building {ContentCount} content routes in {BuildTimeMs} ms",
                        builder.GetSiteState().ContentRoutes.Count,
                        buildTimer.ElapsedMilliseconds);
                    break;

                case "help":
                    options.WriteOptionDescriptions(Console.Out);
                    break;

                case "host":
                    LoadSiteRoutes(logger, builder);

                    logger.LogInformation(
                        "Hosting site at {HostOriginAndPath}",
                        hostOrigin + pathPrefix);
                    var hostLogger = loggerFactory.CreateLogger<HostLocalSite>();
                    HostLocalSite.Host(hostLogger, builder.GetSiteState(), hostOrigin, pathPrefix);
                    break;

                case "new":
                    if (string.IsNullOrEmpty(newPostPath))
                    {
                        throw new ArgumentException("Missing or empty --new-post");
                    }

                    var newPostParentDir = Path.GetDirectoryName(newPostPath);
                    if (!Directory.Exists(newPostParentDir))
                    {
                        Directory.CreateDirectory(newPostParentDir);
                    }

                    var newPostDate = DateTimeOffset.Now.ToString("o");

                    File.WriteAllText(
                        newPostPath,
                        @$"+++
date = ""{newPostDate}""
draft = true
title = """"
category = [""Playing A Game""]
game = []
platform = []
rating = []
+++
",
                        outputEncoding);
                    logger.LogInformation(
                        "Created a new post file at {NewPostPath}",
                        newPostPath);
                    break;

                case "reportstats":
                    if (string.IsNullOrEmpty(reportPath))
                    {
                        throw new ArgumentException("Missing or empty --report-path");
                    }

                    var reportStartDate = DateTimeOffset.Parse(reportStartDateString, CultureInfo.InvariantCulture);
                    var reportEndDate = DateTimeOffset.Parse(reportEndDateString, CultureInfo.InvariantCulture);

                    var reportStats = builder.GetGameStatsForDateRange(reportStartDate, reportEndDate);

                    // TODO: encapsulated CSV serializer!
                    var reportTextBuilder = new StringBuilder();
                    reportTextBuilder.AppendLine("Title,Platform,Type,Firstposted,Lastposted,Rating,Numposts");
                    foreach (var reportStat in reportStats)
                    {
                        reportTextBuilder.Append("\"");
                        reportTextBuilder.Append(reportStat.Title);
                        reportTextBuilder.Append("\",");
                        reportTextBuilder.Append(reportStat.Platform);
                        reportTextBuilder.Append(",");
                        reportTextBuilder.Append(reportStat.Type);
                        reportTextBuilder.Append(",");
                        reportTextBuilder.Append(reportStat.FirstPosted?.ToString("o"));
                        reportTextBuilder.Append(",");
                        reportTextBuilder.Append(reportStat.LastPosted?.ToString("o"));
                        reportTextBuilder.Append(",");
                        reportTextBuilder.Append(string.IsNullOrEmpty(reportStat.Rating) ? "N/A" : reportStat.Rating);
                        reportTextBuilder.Append(",");
                        reportTextBuilder.Append(reportStat.NumPosts.ToString(CultureInfo.InvariantCulture));

                        reportTextBuilder.AppendLine();
                    }

                    File.WriteAllText(reportPath, reportTextBuilder.ToString(), outputEncoding);
                    logger.LogInformation(
                        "Generated report from {ReportStartDate} to {ReportEndDate} at {ReportPath}",
                        reportStartDate,
                        reportEndDate,
                        reportPath);
                    break;

                case "undraft":
                    if (string.IsNullOrEmpty(newPostPath))
                    {
                        throw new ArgumentException("Missing or empty --new-post");
                    }

                    if (!File.Exists(newPostPath))
                    {
                        throw new ArgumentException($"Post path doesn't exist: {newPostPath}");
                    }

                    var undraftDate = DateTimeOffset.Now.ToString("o");

                    var postText = File.ReadAllText(newPostPath);
                    postText = Regex.Replace(postText, @"draft = true\s*", string.Empty);
                    postText = Regex.Replace(postText, @"date = ""[^""]*""", $"date = \"{undraftDate}\"");

                    File.WriteAllText(newPostPath, postText, outputEncoding);
                    logger.LogInformation(
                        "Undrafted post file at {NewPostPath}",
                        newPostPath);
                    break;

                default:
                    throw new ArgumentException($"Unknown verb `{activeVerb}`");
            }

            return 0;
        }

        private static void LoadSiteData(ILogger logger, SiteBuilder builder)
        {
            logger.LogInformation("Loading data...");
            var loadTimer = Stopwatch.StartNew();
            builder.UpdateDataIndex();
            loadTimer.Stop();
            logger.LogInformation(
                "Finished loading site data in {LoadTimeMs} ms",
                loadTimer.ElapsedMilliseconds);
        }

        private static void LoadSiteRoutes(ILogger logger, SiteBuilder builder)
        {
            logger.LogInformation("Loading routes...");
            var loadTimer = Stopwatch.StartNew();
            builder.UpdateContentRoutes();
            loadTimer.Stop();
            logger.LogInformation(
                "Finished loading {ContentCount} content routes in {LoadTimeMs} ms",
                builder.GetSiteState().ContentRoutes.Count,
                loadTimer.ElapsedMilliseconds);
        }
    }
}
