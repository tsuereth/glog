using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GlogGenerator.IgdbApi;
using GlogGenerator.RenderState;
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
            var staticSiteOutputBasePath = Path.Combine(Directory.GetCurrentDirectory(), "public");
            var updateIgdbCache = false;

            var options = new OptionSet()
            {
                { "igdb-client-id=", "IGDB API (Twitch Developers) Client ID", o => igdbClientId = o },
                { "igdb-client-secret=", "IGDB API (Twitch Developers) Client Secret", o => igdbClientSecret = o },
                { "i|input-path=", $"Input files base path, default: {inputFilesBasePath}", o => inputFilesBasePath = o },
                { "t|templates-path=", $"Template files base path, default: {templateFilesBasePath}", o => templateFilesBasePath = o },
                { "h|host-origin=", $"Host origin (scheme + name + port) for the site, default: {hostOrigin}", o => hostOrigin = o },
                { "n|new-post=", $"NEW|UNDRAFT: local path to a new/draft post file", o => newPostPath = o },
                { "p|path-prefix=", $"Path prefix, default: {pathPrefix}", o => pathPrefix = o },
                { "o|output-path=", $"BUILD: Static site output base path, default: {staticSiteOutputBasePath}", o => staticSiteOutputBasePath = o },
                { "u|update-igdb-cache=", $"Update the IGDB data cache (requires IGDB API credentials), default: {updateIgdbCache}", o => updateIgdbCache = bool.Parse(o) },
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

            var site = SiteState.FromInputFilesBasePath(inputFilesBasePath, templateFilesBasePath);

            site.BaseURL = $"{hostOrigin}{pathPrefix}"; // TODO: ensure proper slash-usage between origin and path

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

                using (var igdbApiClient = new IgdbApiClient(igdbClientId, igdbClientSecret))
                {
                    logger.LogInformation("Updating IGDB cache...");
                    var cacheUpdateTimer = Stopwatch.StartNew();
                    await site.IgdbCache.UpdateFromApiClient(igdbApiClient);
                    cacheUpdateTimer.Stop();
                    logger.LogInformation(
                        "Finished updating in {CacheUpdateTimeMs} ms",
                        cacheUpdateTimer.ElapsedMilliseconds);
                }

                site.IgdbCache.WriteToJsonFile(inputFilesBasePath);
            }

            var outputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            switch (activeVerb.ToLowerInvariant())
            {
                case "build":
                    LoadSiteContent(logger, site);

                    logger.LogInformation("Building content...");
                    var buildTimer = Stopwatch.StartNew();
                    BuildStaticSite.Build(site, staticSiteOutputBasePath);
                    buildTimer.Stop();
                    logger.LogInformation(
                        "Finished building {ContentCount} content routes in {BuildTimeMs} ms",
                        site.ContentRoutes.Count,
                        buildTimer.ElapsedMilliseconds);
                    break;

                case "host":
                    LoadSiteContent(logger, site);

                    logger.LogInformation(
                        "Hosting site at {HostOriginAndPath}",
                        hostOrigin + pathPrefix);
                    var hostLogger = loggerFactory.CreateLogger<HostLocalSite>();
                    HostLocalSite.Host(hostLogger, site, hostOrigin, pathPrefix);
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

        private static void LoadSiteContent(ILogger logger, SiteState site)
        {
            logger.LogInformation("Loading content...");
            var loadTimer = Stopwatch.StartNew();
            site.LoadContent();
            loadTimer.Stop();
            logger.LogInformation(
                "Finished loading {ContentCount} content routes in {LoadTimeMs} ms",
                site.ContentRoutes.Count,
                loadTimer.ElapsedMilliseconds);
        }
    }
}
