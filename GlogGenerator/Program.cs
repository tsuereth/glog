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
        public enum NonBuildVerbs
        {
            AddGames,
            Help,
            New,
            Undraft,
        };

        static async Task<int> Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(c => c.AddConsole());

            var logger = loggerFactory.CreateLogger<Program>();

            var projectProperties = Assembly.GetEntryAssembly().GetCustomAttribute<ProjectPropertiesAttribute>();

            var igdbClientId = string.Empty;
            var igdbClientSecret = string.Empty;
            var addIgdbGameIdsListString = string.Empty;
            var siteIndexFilesBasePath = projectProperties.DefaultSiteIndexFilesBasePath;
            var igdbCacheFilesBasePath = projectProperties.DefaultIgdbCacheFilesBasePath;
            var inputFilesBasePath = projectProperties.DefaultInputFilesBasePath;
            var includeDraftsString = SiteBuilder.IncludeDrafts.HostModeOnly.ToString();
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

            var verbOptions = new List<string>();
            verbOptions.AddRange(Enum.GetNames<NonBuildVerbs>());
            verbOptions.AddRange(Enum.GetNames<SiteBuilder.Mode>());
            verbOptions.Sort();
            var verbOptionsString = string.Join(',', verbOptions);

            var includeDraftsOptionsString = string.Join(',', Enum.GetNames<SiteBuilder.IncludeDrafts>());

            var options = new OptionSet()
            {
                { "igdb-client-id=", "IGDB API (Twitch Developers) Client ID", o => igdbClientId = o },
                { "igdb-client-secret=", "IGDB API (Twitch Developers) Client Secret", o => igdbClientSecret = o },
                { "add-igdb-game-ids=", "Comma-separated IGDB Game IDs to optionally add to the data cache", o => addIgdbGameIdsListString = o },
                { "index-files-path=", $"Site data index files base path, default: {siteIndexFilesBasePath}", o => siteIndexFilesBasePath = o },
                { "cache-files-path=", $"IGDB cache files base path, default: {igdbCacheFilesBasePath}", o => igdbCacheFilesBasePath = o },
                { "i|input-path=", $"Input files base path, default: {inputFilesBasePath}", o => inputFilesBasePath = o },
                { "d|include-drafts=", $"When to include draft content (one of {includeDraftsOptionsString}), default: {includeDraftsString}", o => includeDraftsString = o },
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
            var verbStrings = options.Parse(args);

            object activeVerb = SiteBuilder.Mode.Host;
            if (verbStrings.Count == 0)
            {
                logger.LogWarning(
                    "No verbs provided, defaulting to `{ActiveVerb}`",
                    activeVerb);
            }
            else
            {
                var activeVerbString = verbStrings[0];
                if (verbStrings.Count > 1)
                {
                    logger.LogInformation(
                        "More than one verb was provided, accepting `{ActiveVerbString}` and ignoring the following: {IgnoredVerbs}",
                        activeVerbString,
                        string.Join(", ", verbStrings.GetRange(1, verbStrings.Count - 1)));
                }

                if (Enum.TryParse<NonBuildVerbs>(activeVerbString, ignoreCase: true, out var nonBuildVerb))
                {
                    activeVerb = nonBuildVerb;
                }
                else if (Enum.TryParse<SiteBuilder.Mode>(activeVerbString, ignoreCase: true, out var siteBuilderModeVerb))
                {
                    activeVerb = siteBuilderModeVerb;
                }
                else
                {
                    throw new ArgumentException($"Unrecognized verb {activeVerbString}, must be one of {verbOptionsString}");
                }
            }

            if (!Enum.TryParse<SiteBuilder.IncludeDrafts>(includeDraftsString, ignoreCase: true, out var includeDrafts))
            {
                throw new ArgumentException($"Unrecognized --include-drafts argument {includeDraftsString}, must be one of {includeDraftsOptionsString}");
            }

            var configFilePath = Path.Combine(inputFilesBasePath, "config.toml");
            var configData = ConfigData.FromFilePaths(
                configFilePath,
                siteIndexFilesBasePath,
                igdbCacheFilesBasePath,
                inputFilesBasePath,
                templateFilesBasePath);
            var builder = new SiteBuilder(logger, configData);
            builder.SetBaseURL($"{hostOrigin}{pathPrefix}"); // TODO: ensure proper slash-usage between origin and path

            var activeVerbMustLoadSiteData = false;
            if (activeVerb is SiteBuilder.Mode siteBuilderMode)
            {
                builder.SetMode(siteBuilderMode, includeDrafts);
                activeVerbMustLoadSiteData = true;
            }

            var addIgdbGameIdsStrings = addIgdbGameIdsListString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var addIgdbGameIds = addIgdbGameIdsStrings.Select(s =>
            {
                if (!int.TryParse(s, out var addIgdbGameId))
                {
                    throw new ArgumentException($"Failed to parse additional IGDB game ID '{s}'");
                }
                return addIgdbGameId;
            }).ToList();
            if (addIgdbGameIds.Count > 0)
            {
                builder.SetAdditionalIgdbGameIds(addIgdbGameIds);
            }

            if (activeVerbMustLoadSiteData || updateIgdbCache || rewriteInputFiles)
            {
                var loadedIgdbCache = InitializeSiteData(logger, builder);
                if (loadedIgdbCache)
                {
                    // Add the cached IGDB data to the site data index.
                    LoadSiteData(logger, builder);
                }
                else
                {
                    logger.LogInformation("No IGDB cache was loaded, data must be fetched from IGDB");
                    updateIgdbCache = true;
                }
            }

            if (updateIgdbCache || activeVerb.Equals(NonBuildVerbs.AddGames))
            {
                if (string.IsNullOrEmpty(igdbClientId))
                {
                    throw new ArgumentException("Missing or empty --igdb-client-id");
                }
                if (string.IsNullOrEmpty(igdbClientSecret))
                {
                    throw new ArgumentException("Missing or empty --igdb-client-secret");
                }

                // Ensure that data references are resolved to underlying IDs,
                // before potentially modifying data keys with this update.
                builder.ResolveDataReferences();

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

            if (activeVerbMustLoadSiteData || updateIgdbCache || rewriteInputFiles)
            {
                // Resolve all references to the most-current data.
                builder.ResolveDataReferences();
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

            switch (activeVerb)
            {
                case NonBuildVerbs.AddGames:
                    // The --add-igdb-game-ids values have already been added, there's nothing to do here.
                    break;

                case NonBuildVerbs.Help:
                    options.WriteOptionDescriptions(Console.Out);
                    break;

                case NonBuildVerbs.New:
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
category = [ ""{CategoryData.PlayingAGameCategoryName}"" ]
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

                case NonBuildVerbs.Undraft:
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

                case SiteBuilder.Mode.UpdateDataNoOutput:
                    LoadSiteRoutes(logger, builder);
                    break;

                case SiteBuilder.Mode.Build:
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

                case SiteBuilder.Mode.Host:
                    LoadSiteRoutes(logger, builder);

                    logger.LogInformation(
                        "Hosting site at {HostOriginAndPath}",
                        hostOrigin + pathPrefix);
                    var hostLogger = loggerFactory.CreateLogger<HostLocalSite>();
                    HostLocalSite.Host(hostLogger, builder.GetSiteState(), hostOrigin, pathPrefix);
                    break;

                case SiteBuilder.Mode.ReportStats:
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

                default:
                    throw new ArgumentException($"Unhandled verb `{activeVerb}`");
            }

            if (activeVerbMustLoadSiteData || updateIgdbCache || rewriteInputFiles)
            {
                // After the site's been built, an updated data index and cache (pruned of unused data) can be rewritten.
                builder.RewriteSiteDataIndexFiles();
                builder.RewriteIgdbCache();
            }

            return 0;
        }

        private static bool InitializeSiteData(ILogger logger, SiteBuilder builder)
        {
            logger.LogInformation("Initializing data...");

            var loadIndexTimer = Stopwatch.StartNew();
            builder.LoadSiteDataIndexFiles();
            loadIndexTimer.Stop();
            logger.LogInformation(
                "Finished initializing site data index in {LoadTimeMs} ms",
                loadIndexTimer.ElapsedMilliseconds);

            var loadCacheTimer = Stopwatch.StartNew();
            var loadedCache = builder.TryLoadIgdbCache();
            loadCacheTimer.Stop();
            if (loadedCache)
            {
                logger.LogInformation(
                    "Finished initializing site IGDB cache in {LoadTimeMs} ms",
                    loadCacheTimer.ElapsedMilliseconds);
            }

            return loadedCache;
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
