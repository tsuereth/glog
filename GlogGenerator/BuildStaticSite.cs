using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiffPlex.Renderer;
using GlogGenerator.RenderState;

namespace GlogGenerator
{
    public static class BuildStaticSite
    {
        public static void Build(
            SiteState site,
            string outputBasePath)
        {
            foreach (var contentRoute in site.ContentRoutes)
            {
                var outputFilePath = Path.Combine(outputBasePath, contentRoute.Key);
                var content = contentRoute.Value;

                content.WriteFile(site, outputFilePath);
            }
        }

        public static void CompareTo(
            SiteState site,
            string compareToBasePath,
            Stream outputStream)
        {
            // Keep track of every file in the compare-to base path.
            // Content which wasn't found (and diffed) in the current site
            // will yield an "Only in ..." line in the diff output.
            var compareToContentRoutes = new HashSet<string>();
            foreach (var compareToFilePath in Directory.EnumerateFiles(compareToBasePath, "*.*", SearchOption.AllDirectories))
            {
                // Remove the base path and normalize directory separators.
                var contentRoute = compareToFilePath.Substring(compareToBasePath.Length);
                if (contentRoute[0] == Path.DirectorySeparatorChar)
                {
                    contentRoute = contentRoute.Substring(1);
                }

                // Runtime content routes should ALWAYS use unix-style path separators '/'
                contentRoute = contentRoute.Replace(Path.DirectorySeparatorChar, '/');
                compareToContentRoutes.Add(contentRoute);
            }

            // TODO: NOT THIS! Not a stringbuilder of results from each file's diff!
            // Instead, track diff-line batches <-> the files relevant to them
            // I.e. after extracting diff-lines from 3.html, recognize that Batch N
            // matches previously seen diff-lines from 1.html and 2.html
            var diffBuilder = new StringBuilder();

            var onlyInCurrentSiteRoutes = new HashSet<string>();

            foreach (var contentRoute in site.ContentRoutes.OrderBy(r => r.Key))
            {
                if (compareToContentRoutes.Contains(contentRoute.Key))
                {
                    compareToContentRoutes.Remove(contentRoute.Key);

                    var compareToFilePath = Path.Combine(compareToBasePath, contentRoute.Key).Replace(Path.DirectorySeparatorChar, '/');

                    if (contentRoute.Value.GetContentTypeIsText())
                    {
                        var compareToText = File.ReadAllText(compareToFilePath);
                        var currentText = contentRoute.Value.GetText(site);

                        var unidiff = UnidiffRenderer.GenerateUnidiff(compareToText, currentText, contentRoute.Key, contentRoute.Key);
                        if (!string.IsNullOrEmpty(unidiff))
                        {
                            diffBuilder.Append(unidiff);
                        }
                    }
                    else
                    {
                        var compareToBytes = File.ReadAllBytes(compareToFilePath);
                        var currentBytes = contentRoute.Value.GetBytes(site);

                        if (!compareToBytes.SequenceEqual(currentBytes))
                        {
                            var binaryDiffLine = $"Binary files {contentRoute.Key} and {contentRoute.Key} differ";
                            diffBuilder.AppendLine(binaryDiffLine);
                        }
                    }
                }
                else
                {
                    onlyInCurrentSiteRoutes.Add(contentRoute.Key);
                }
            }

            foreach (var compareToRouteNotFound in compareToContentRoutes.OrderBy(r => r))
            {
                var onlyInCompareToLine = $"Only in compare-to: {compareToRouteNotFound}";
                diffBuilder.AppendLine(onlyInCompareToLine);
            }

            foreach (var currentSiteRouteNotFound in onlyInCurrentSiteRoutes.OrderBy(r => r))
            {
                var onlyInCurrentSiteLine = $"Only in {site.BaseURL}: {currentSiteRouteNotFound}";
                diffBuilder.AppendLine(onlyInCurrentSiteLine);
            }

            if (diffBuilder.Length == 0)
            {
                diffBuilder.AppendLine("No differences found.");
            }

            // `leaveOpen` refers to the given outputStream; let the caller decide when to close that.
            using (var diffWriter = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true))
            {
                diffWriter.Write(diffBuilder);
                diffWriter.Flush();
            }
        }
    }
}
