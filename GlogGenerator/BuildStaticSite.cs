using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GlogGenerator.DiffSummary;
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

                // Skip files which begin with "page/..." because these diffs are largely just
                // pagination shifts: a new post "shifting" an older post out of page N, and into page N+1.
                // (FIXME?: can diff summarization detect these content moves as a pagination change?)
                if (contentRoute.StartsWith("page/", StringComparison.InvariantCulture))
                {
                    continue;
                }

                compareToContentRoutes.Add(contentRoute);
            }

            var diffSummaryTracker = new DiffSummaryTracker();
            foreach (var contentRoute in site.ContentRoutes)
            {
                // Skip files which begin with "page/..." -- see note above.
                if (contentRoute.Key.StartsWith("page/", StringComparison.InvariantCulture))
                {
                    continue;
                }

                if (compareToContentRoutes.Contains(contentRoute.Key))
                {
                    compareToContentRoutes.Remove(contentRoute.Key);

                    var compareToFilePath = Path.Combine(compareToBasePath, contentRoute.Key).Replace(Path.DirectorySeparatorChar, '/');

                    if (contentRoute.Value.GetContentTypeIsText())
                    {
                        var compareToText = File.ReadAllText(compareToFilePath);
                        var currentText = contentRoute.Value.GetText(site);

                        diffSummaryTracker.AddTextFile(contentRoute.Key, compareToText, currentText);
                    }
                    else
                    {
                        var compareToBytes = File.ReadAllBytes(compareToFilePath);
                        var currentBytes = contentRoute.Value.GetBytes(site);

                        diffSummaryTracker.AddBinaryFile(contentRoute.Key, compareToBytes, currentBytes);
                    }
                }
                else
                {
                    diffSummaryTracker.AddFileOnlyInCurrent(contentRoute.Key);
                }
            }

            foreach (var compareToRouteNotFound in compareToContentRoutes)
            {
                diffSummaryTracker.AddFileOnlyInCompareTo(compareToRouteNotFound);
            }

            diffSummaryTracker.WriteSummary(outputStream);
            outputStream.Flush();
        }
    }
}
