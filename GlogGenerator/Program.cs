using System.IO;
using Mono.Options;

namespace GlogGenerator
{
    public static class Program
    {
        static int Main(string[] args)
        {
            var inputFilesBasePath = Directory.GetCurrentDirectory();
            var templateFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "templates");
            var staticSiteOutputBasePath = Path.Combine(Directory.GetCurrentDirectory(), "public");

            // TODO: "mode" for hosting locally vs. building the static site.
            var options = new OptionSet()
            {
                { "i|input-path=", "Input files base path", o => inputFilesBasePath = o },
                { "t|templates-path=", "Template files base path", o => templateFilesBasePath = o },
                { "o|output-path=", "Static site output base path", o => staticSiteOutputBasePath = o },
            };
            options.Parse(args);

            BuildStaticSite.Build(inputFilesBasePath, templateFilesBasePath, staticSiteOutputBasePath);

            return 0;
        }
    }
}
