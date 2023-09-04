using System;
using System.Globalization;
using System.IO;
using System.Linq;
using GlogGenerator.RenderState;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test
{
    [TestClass]
    public class SmallSiteTest
    {
        [TestMethod]
        public void TestBuildStaticSite()
        {
            var inputFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "SmallSiteTest");
            var templateFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "templates");
            var hostOrigin = "http://fakeorigin.com";
            var pathPrefix = "/glog/";
            var staticSiteOutputBasePath = Path.Combine(Directory.GetCurrentDirectory(), "public");

            var site = SiteState.FromInputFilesBasePath(inputFilesBasePath, templateFilesBasePath);

            site.BaseURL = $"{hostOrigin}{pathPrefix}"; // TODO: ensure proper slash-usage between origin and path

            // For testing, pretend that our "build date" is some constant date.
            site.BuildDate = DateTimeOffset.Parse("2023-09-04T17:00:00.0+00:00", CultureInfo.InvariantCulture);

            site.LoadContent();

            // Ensure the output directory is clean, first.
            Directory.Delete(staticSiteOutputBasePath, recursive: true);
            BuildStaticSite.Build(site, staticSiteOutputBasePath);

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
