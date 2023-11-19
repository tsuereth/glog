using System.Collections.Generic;
using System.IO;
using GlogGenerator.Data;
using GlogGenerator.IgdbApi;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace GlogGenerator.Test.Data
{
    [TestClass]
    public class PostDataTests
    {
        [TestMethod]
        public void TestFromFilePath()
        {
            var logger = new TestLogger();

            var testPostFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "Data", "PostDataTests", "testpostdata.md");

            var siteDataIndex = new FakeSiteDataIndex();
            var builder = new SiteBuilder(logger, new ConfigData(), siteDataIndex);
            var testPostData = PostData.MarkdownFromFilePath(builder.GetMarkdownPipeline(), testPostFilePath, siteDataIndex);

            Assert.AreEqual("2023/07/23/re-climbing-the-orc-chart/", testPostData.PermalinkRelative);
        }

        [TestMethod]
        public void TestToMarkdownStringSimple()
        {
            var logger = new TestLogger();

            var testPostFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "Data", "PostDataTests", "testpostdata.md");
            var testPostFileText = File.ReadAllText(testPostFilePath);

            var siteDataIndex = new FakeSiteDataIndex();
            var builder = new SiteBuilder(logger, new ConfigData(), siteDataIndex);
            var testPostData = PostData.MarkdownFromFilePath(builder.GetMarkdownPipeline(), testPostFilePath, siteDataIndex);
            var testPostToString = testPostData.ToMarkdownString(builder.GetMarkdownPipeline(), siteDataIndex);

            Assert.AreEqual(testPostFileText, testPostToString);
        }

        [TestMethod]
        public void TestToMarkdownStringComplex()
        {
            var logger = new TestLogger();

            var testPostFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "Data", "PostDataTests", "testfenceddatacharts.md");
            var testPostFileText = File.ReadAllText(testPostFilePath);

            var siteDataIndex = new FakeSiteDataIndex();
            var builder = new SiteBuilder(logger, new ConfigData(), siteDataIndex);
            var testPostData = PostData.MarkdownFromFilePath(builder.GetMarkdownPipeline(), testPostFilePath, siteDataIndex);
            var testPostToString = testPostData.ToMarkdownString(builder.GetMarkdownPipeline(), siteDataIndex);

            Assert.AreEqual(testPostFileText, testPostToString);
        }

        [TestMethod]
        public void TestToMarkdownAfterKeyChanges()
        {
            var logger = new TestLogger();

            var testPostFileText = @"+++
game = [ ""Middle-earth: Shadow of Mordor"" ]
+++
<i>Oh yeah</i>, there's a new [Lord of the Rings game](game:The Lord of the Rings: Gollum) out!";

            var testIgdbGameShadowOfMordor = new IgdbGame() { Id = 1, Name = "Middle-earth: Shadow of Mordor" };
            var testIgdbGameGollum = new IgdbGame() { Id = 4, Name = "The Lord of the Rings: Gollum" };

            var mockIgdbCache = Substitute.For<IIgdbCache>();
            mockIgdbCache.GetAllGames().Returns(new List<IgdbGame>() { testIgdbGameShadowOfMordor, testIgdbGameGollum });
            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>());
            mockIgdbCache.GetAllGameMetadata().Returns(new List<IgdbEntity>());

            var testIndex = new SiteDataIndex(logger, string.Empty);
            var builder = new SiteBuilder(logger, new ConfigData(), testIndex);

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            var testPostData = PostData.MarkdownFromString(builder.GetMarkdownPipeline(), testPostFileText, testIndex);
            testIndex.ResolveReferences();

            testIgdbGameShadowOfMordor.Name = "Assassin's Creed Mordor";
            testIgdbGameGollum.Name = "Goblin Mode";

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

            var testPostToString = testPostData.ToMarkdownString(builder.GetMarkdownPipeline(), testIndex);

            var expectedText = @"+++
game = [ ""Assassin's Creed Mordor"" ]
+++
<i>Oh yeah</i>, there's a new [Lord of the Rings game](game:Goblin Mode) out!";

            Assert.AreEqual(expectedText, testPostToString);
        }
    }
}
