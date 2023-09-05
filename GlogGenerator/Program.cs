using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GlogGenerator.IgdbApi;
using GlogGenerator.RenderState;
using Mono.Options;

namespace GlogGenerator
{
    public static class Program
    {
        static async Task<int> Main(string[] args)
        {
            var projectProperties = Assembly.GetEntryAssembly().GetCustomAttribute<ProjectPropertiesAttribute>();

            var igdbClientId = string.Empty;
            var igdbClientSecret = string.Empty;
            var inputFilesBasePath = projectProperties.DefaultInputFilesBasePath;
            var templateFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "templates");
            var hostOrigin = "http://localhost:1313";
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
                { "p|path-prefix=", $"Path prefix, default: {pathPrefix}", o => pathPrefix = o },
                { "o|output-path=", $"BUILD: Static site output base path, default: {staticSiteOutputBasePath}", o => staticSiteOutputBasePath = o },
                { "u|update-igdb-cache=", $"Update the IGDB data cache (requires IGDB API credentials), default: {updateIgdbCache}", o => updateIgdbCache = bool.Parse(o) },
            };
            var verbs = options.Parse(args);

            var activeVerb = string.Empty;
            if (verbs.Count == 0)
            {
                Console.WriteLine("No verbs provided, defaulting to `host`");
                activeVerb = "host";
            }
            else
            {
                activeVerb = verbs[0];
                if (verbs.Count > 1)
                {
                    Console.WriteLine($"More than one verb was provided, using `{activeVerb}` and ignoring the following: {string.Join(", ", verbs.GetRange(1, verbs.Count - 1))}");
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
                    await site.IgdbCache.UpdateFromApiClient(igdbApiClient);
                }

                var igdbCacheFilesDirectory = Path.Combine(inputFilesBasePath, IgdbCache.JsonFilesBaseDir);
                site.IgdbCache.WriteToJsonFiles(igdbCacheFilesDirectory);
            }

            Console.WriteLine("Loading content...");
            var loadTimer = Stopwatch.StartNew();
            site.LoadContent();
            loadTimer.Stop();
            Console.WriteLine($"Finished loading in {loadTimer.ElapsedMilliseconds} ms");

            switch (activeVerb.ToLowerInvariant())
            {
                case "build":
                    BuildStaticSite.Build(site, staticSiteOutputBasePath);
                    break;

                case "host":
                    HostLocalSite.Host(site, hostOrigin, pathPrefix);
                    break;

                default:
                    throw new ArgumentException($"Unknown verb `{activeVerb}`");
            }

            return 0;
        }
    }
}
