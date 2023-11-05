using System.Collections.Generic;
using GlogGenerator.Data;
using GlogGenerator.IgdbApi;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace GlogGenerator.Test.Data
{
    [TestClass]
    public class SiteDataIndexTests
    {
        [TestMethod]
        public void TestLoadContentMergedTags()
        {
            var logger = new TestLogger();

            var testGameMetadata = new List<IgdbEntity>()
            {
                new IgdbCollection() { Name = "Sonic The Hedgehog" },
                new IgdbFranchise() { Name = "Sonic the Hedgehog" },
            };

            var mockIgdbCache = Substitute.For<IIgdbCache>();
            mockIgdbCache.GetAllGames().Returns(new List<IgdbGame>());
            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>());
            mockIgdbCache.GetAllGameMetadata().Returns(testGameMetadata);

            var mockSiteBuilder = Substitute.For<ISiteBuilder>();
            var testIndex = new SiteDataIndex(logger, mockSiteBuilder, string.Empty);

            testIndex.LoadContent(mockIgdbCache);

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            var testTag = testIndex.GetTag("Sonic the Hedgehog");
            Assert.AreEqual("Sonic The Hedgehog", testTag.Name);
        }

        [TestMethod]
        public void TestLoadContentUpdateMissingKey()
        {
            var logger = new TestLogger();

            var testPlatformsOld = new List<IgdbPlatform>()
            {
                new IgdbPlatform() { Abbreviation = "PS1" },
            };

            var mockIgdbCache = Substitute.For<IIgdbCache>();
            mockIgdbCache.GetAllGames().Returns(new List<IgdbGame>());
            mockIgdbCache.GetAllPlatforms().Returns(testPlatformsOld);
            mockIgdbCache.GetAllGameMetadata().Returns(new List<IgdbEntity>());

            var mockSiteBuilder = Substitute.For<ISiteBuilder>();
            var testIndex = new SiteDataIndex(logger, mockSiteBuilder, string.Empty);

            testIndex.LoadContent(mockIgdbCache);

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            var testPlatformsNew = new List<IgdbPlatform>()
            {
                new IgdbPlatform() { Abbreviation = "PSOne" },
            };

            mockIgdbCache.GetAllPlatforms().Returns(testPlatformsNew);

            testIndex.LoadContent(mockIgdbCache);

            var errors = logger.GetLogs(LogLevel.Error);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Updated data index is missing old PlatformData key ps1 permalink platform/ps1/", errors[0].Message);

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);
        }
    }
}
