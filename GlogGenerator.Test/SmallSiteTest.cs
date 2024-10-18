using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GlogGenerator.Data;
using GlogGenerator.MarkdownExtensions;
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
            var builder = new SiteBuilder(logger, configData);
            builder.SetBaseURL($"{hostOrigin}{pathPrefix}"); // TODO: ensure proper slash-usage between origin and path

            // For testing, pretend that our "build date" is some constant date.
            builder.SetBuildDate(DateTimeOffset.Parse("2024-09-14T17:00:00.0+00:00", CultureInfo.InvariantCulture));

            builder.UpdateDataIndex();

            builder.ResolveDataReferences();

            builder.UpdateContentRoutes();

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

        [TestMethod]
        public void TestBuildStaticSiteWithReloadedData()
        {
            using var loggerFactory = LoggerFactory.Create(c => c.AddConsole());

            var logger = loggerFactory.CreateLogger<Program>();

            var inputFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "SmallSiteTest");
            var templateFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "templates");
            var hostOrigin = "http://fakeorigin.com";
            var pathPrefix = "/glog/";
            var staticSiteOutputBasePath = Path.Combine(Directory.GetCurrentDirectory(), "public-reload");

            var configFilePath = Path.Combine(inputFilesBasePath, "config.toml");
            var configData = ConfigData.FromFilePaths(configFilePath, inputFilesBasePath, templateFilesBasePath);
            var builder = new SiteBuilder(logger, configData);
            builder.SetBaseURL($"{hostOrigin}{pathPrefix}"); // TODO: ensure proper slash-usage between origin and path

            // For testing, pretend that our "build date" is some constant date.
            builder.SetBuildDate(DateTimeOffset.Parse("2024-09-14T17:00:00.0+00:00", CultureInfo.InvariantCulture));

            builder.UpdateDataIndex();

            // Simulate the data-update flow by re-loading the data index.
            builder.ResolveDataReferences();
            builder.UpdateDataIndex();

            builder.UpdateContentRoutes();

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

        [TestMethod]
        public void TestRewriteInputFiles()
        {
            using var loggerFactory = LoggerFactory.Create(c => c.AddConsole());

            var logger = loggerFactory.CreateLogger<Program>();

            var inputFilesBasePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "SmallSiteTest");
            var templateFilesBasePath = string.Empty;
            var staticSiteOutputBasePath = string.Empty;

            var configFilePath = Path.Combine(inputFilesBasePath, "config.toml");
            var configData = ConfigData.FromFilePaths(configFilePath, inputFilesBasePath, templateFilesBasePath);
            var builder = new SiteBuilder(logger, configData);

            builder.UpdateDataIndex();

            foreach (var page in builder.GetPages())
            {
                var rewrittenFileText = page.MdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

                var expectedFileBytes = File.ReadAllBytes(page.SourceFilePath);
                var actualFileBytes = Encoding.UTF8.GetBytes(rewrittenFileText);
                CollectionAssert.AreEqual(expectedFileBytes, actualFileBytes, $"Output file {page.SourceFilePath} contents differed from expected");
            }

            foreach (var post in builder.GetPosts())
            {
                var rewrittenFileText = post.MdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

                var expectedFileBytes = File.ReadAllBytes(post.SourceFilePath);
                var actualFileBytes = Encoding.UTF8.GetBytes(rewrittenFileText);
                CollectionAssert.AreEqual(expectedFileBytes, actualFileBytes, $"Output file {post.SourceFilePath} contents differed from expected");
            }
        }
    }
}
