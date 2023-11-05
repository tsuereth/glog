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
        public void TestLoadContentUpdateKeyChanged()
        {
            var logger = new TestLogger();

            var testPlatformOld = new IgdbPlatform() { Id = 1, Abbreviation = "PS1" };

            var mockIgdbCache = Substitute.For<IIgdbCache>();
            mockIgdbCache.GetAllGames().Returns(new List<IgdbGame>());
            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>() { testPlatformOld });
            mockIgdbCache.GetAllGameMetadata().Returns(new List<IgdbEntity>());

            var mockSiteBuilder = Substitute.For<ISiteBuilder>();
            var testIndex = new SiteDataIndex(logger, mockSiteBuilder, string.Empty);

            testIndex.LoadContent(mockIgdbCache);

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            var testPlatformNew = new IgdbPlatform() { Id = 1, Abbreviation = "PSOne" };

            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>() { testPlatformNew });

            testIndex.LoadContent(mockIgdbCache);

            var errors = logger.GetLogs(LogLevel.Error);
            Assert.AreEqual(1, errors.Count);

            var expectedMessage = $"Updated data index has a different key for PlatformData with data ID {testPlatformOld.GetUniqueIdString()}: old key {testPlatformOld.GetReferenceableKey()} new key {testPlatformNew.GetReferenceableKey()}";
            Assert.AreEqual(expectedMessage, errors[0].Message);

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);
        }

        [TestMethod]
        public void TestLoadContentUpdateMissingKey()
        {
            var logger = new TestLogger();

            var testPlatformOld = new IgdbPlatform() { Id = 1, Abbreviation = "PS1" };

            var mockIgdbCache = Substitute.For<IIgdbCache>();
            mockIgdbCache.GetAllGames().Returns(new List<IgdbGame>());
            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>() { testPlatformOld });
            mockIgdbCache.GetAllGameMetadata().Returns(new List<IgdbEntity>());

            var mockSiteBuilder = Substitute.For<ISiteBuilder>();
            var testIndex = new SiteDataIndex(logger, mockSiteBuilder, string.Empty);

            testIndex.LoadContent(mockIgdbCache);

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>());

            testIndex.LoadContent(mockIgdbCache);

            var errors = logger.GetLogs(LogLevel.Error);
            Assert.AreEqual(1, errors.Count);

            var expectedMessage = $"Updated data index is missing old PlatformData with data ID {testPlatformOld.GetUniqueIdString()} and key {testPlatformOld.GetReferenceableKey()}";
            Assert.AreEqual(expectedMessage, errors[0].Message);

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);
        }

        [TestMethod]
        public void TestLoadContentUpdateDataIdChanged()
        {
            var logger = new TestLogger();

            var testPlatformOld = new IgdbPlatform() { Id = 1, Abbreviation = "PS1" };

            var mockIgdbCache = Substitute.For<IIgdbCache>();
            mockIgdbCache.GetAllGames().Returns(new List<IgdbGame>());
            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>() { testPlatformOld });
            mockIgdbCache.GetAllGameMetadata().Returns(new List<IgdbEntity>());

            var mockSiteBuilder = Substitute.For<ISiteBuilder>();
            var testIndex = new SiteDataIndex(logger, mockSiteBuilder, string.Empty);

            testIndex.LoadContent(mockIgdbCache);

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            var testPlatformNew = new IgdbPlatform() { Id = 2, Abbreviation = "PS1" };

            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>() { testPlatformNew });

            testIndex.LoadContent(mockIgdbCache);

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);

            var warnings = logger.GetLogs(LogLevel.Warning);
            Assert.AreEqual(1, warnings.Count);

            var expectedMessage = $"Updated data index has a different data ID for PlatformData with key {testPlatformOld.GetReferenceableKey()}: old data ID {testPlatformOld.GetUniqueIdString()} new data ID {testPlatformNew.GetUniqueIdString()}";
            Assert.AreEqual(expectedMessage, warnings[0].Message);
        }
    }
}
