using System.IO;
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
    }
}
