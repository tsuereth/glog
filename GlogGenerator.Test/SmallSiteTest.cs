using System;
using System.Globalization;
using System.IO;
using System.Linq;
using GlogGenerator.Data;
using GlogGenerator.RenderState;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test
{
    [TestClass]
    public class SmallSiteTest
    {
        [TestMethod]
        public void TestBuildStaticSite()
        {
            using var loggerFactory = LoggerFactory.Create(c => c.AddConsole());

            var logger = loggerFactory.CreateLogger<Program>();

            var inputFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "SmallSiteTest");
            var templateFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "templates");
            var hostOrigin = "http://fakeorigin.com";
            var pathPrefix = "/glog/";
            var staticSiteOutputBasePath = Path.Combine(Directory.GetCurrentDirectory(), "public");

            var configFilePath = Path.Combine(inputFilesBasePath, "config.toml");
            var configData = ConfigData.FromFilePaths(configFilePath, inputFilesBasePath, templateFilesBasePath);
            var builder = new SiteBuilder(configData);
            builder.SetBaseURL($"{hostOrigin}{pathPrefix}"); // TODO: ensure proper slash-usage between origin and path

            // For testing, pretend that our "build date" is some constant date.
            builder.SetBuildDate(DateTimeOffset.Parse("2023-09-04T17:00:00.0+00:00", CultureInfo.InvariantCulture));

            builder.GetSiteDataIndex().LoadContent();;

            builder.GetSiteState().LoadContentRoutes();

            // Ensure the output directory is clean, first.
            if (Directory.Exists(staticSiteOutputBasePath))
            {
                Directory.Delete(staticSiteOutputBasePath, recursive: true);
            }
            BuildStaticSite.Build(builder.GetSiteState(), staticSiteOutputBasePath);

            var actualFilePaths = Directory.EnumerateFiles(staticSiteOutputBasePath, "*.*", SearchOption.AllDirectories).ToList();
            Assert.IsTrue(actualFilePaths.Count > 0);

            var outputExpected = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "SmallSiteTest", "public-expected");
            var expectedFilePaths = Directory.EnumerateFiles(outputExpected, "*.*", SearchOption.AllDirectories).ToList();
            Assert.AreEqual(expectedFilePaths.Count, actualFilePaths.Count);

            foreach (var expectedFilePath in expectedFilePaths)
            {
                var relativeFilePath = expectedFilePath.Substring(outputExpected.Length + 1); // + 1 for the directory separator
                var actualFilePath = Path.Combine(staticSiteOutputBasePath, relativeFilePath);

                Assert.IsTrue(actualFilePaths.Contains(actualFilePath));

                var expectedFileBytes = File.ReadAllBytes(expectedFilePath);
                var actualFileBytes = File.ReadAllBytes(actualFilePath);
                CollectionAssert.AreEqual(expectedFileBytes, actualFileBytes, $"Output file {relativeFilePath} contents differed from expected");
            }
        }
    }
}
