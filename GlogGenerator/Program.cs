using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using GlogGenerator.RenderState;
using Mono.Options;

namespace GlogGenerator
{
    public static class Program
    {
        static int Main(string[] args)
        {
            var projectProperties = Assembly.GetEntryAssembly().GetCustomAttribute<ProjectPropertiesAttribute>();

            var inputFilesBasePath = projectProperties.DefaultInputFilesBasePath;
            var templateFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "templates");
            var hostOrigin = "http://localhost:1313";
            var pathPrefix = "/glog/";
            var staticSiteOutputBasePath = Path.Combine(Directory.GetCurrentDirectory(), "public");

            var options = new OptionSet()
            {
                { "i|input-path=", $"Input files base path, default: {inputFilesBasePath}", o => inputFilesBasePath = o },
                { "t|templates-path=", $"Template files base path, default: {templateFilesBasePath}", o => templateFilesBasePath = o },
                { "h|host-origin=", $"Host origin (scheme + name + port) for the site, default: {hostOrigin}", o => hostOrigin = o },
                { "p|path-prefix=", $"Path prefix, default: {pathPrefix}", o => pathPrefix = o },
                { "o|output-path=", $"BUILD: Static site output base path, default: {staticSiteOutputBasePath}", o => staticSiteOutputBasePath = o },
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
