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

            var testIndex = new SiteDataIndex(logger, string.Empty);
            var builder = new SiteBuilder(logger, new ConfigData(), testIndex);

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            var testTag = testIndex.GetTag("Sonic the Hedgehog");
            Assert.AreEqual("Sonic The Hedgehog", testTag.Name);

            var testReference = new SiteDataReference<TagData>("Sonic the Hedgehog");
            var testReferenceResolved = testIndex.GetData(testReference);

            Assert.IsTrue(testReference.GetIsResolved());
            Assert.AreEqual("Sonic The Hedgehog", testReferenceResolved.Name);
            Assert.AreEqual(testTag.GetDataId(), testReferenceResolved.GetDataId());
        }

        [TestMethod]
        public void TestLoadContentReferenceConsistentAfterKeyChange()
        {
            var logger = new TestLogger();

            var testDataItem = new IgdbGame() { Id = 1, Name = "Some Game Name" };

            var mockIgdbCache = Substitute.For<IIgdbCache>();
            mockIgdbCache.GetAllGames().Returns(new List<IgdbGame>() { testDataItem });
            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>());
            mockIgdbCache.GetAllGameMetadata().Returns(new List<IgdbEntity>());

            var testIndex = new SiteDataIndex(logger, string.Empty);
            var builder = new SiteBuilder(logger, new ConfigData(), testIndex);

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            var testReference = new SiteDataReference<GameData>("Some Game Name");
            var testData = testIndex.GetData(testReference);

            Assert.IsTrue(testReference.GetIsResolved());
            Assert.AreEqual("Some Game Name", testData.Title);

            testDataItem.Name = "Corrected Game Name";

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

            var errors = logger.GetLogs(LogLevel.Error);
            Assert.AreEqual(1, errors.Count);

            var expectedMessage = $"Updated data index has a different key for GameData with data ID {testDataItem.GetUniqueIdString()}: old key Some Game Name new key Corrected Game Name";
            Assert.AreEqual(expectedMessage, errors[0].Message);

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            Assert.IsTrue(testReference.GetIsResolved());

            var testDataLater = testIndex.GetData(testReference);
            Assert.AreEqual(testData.GetDataId(), testDataLater.GetDataId());
            Assert.AreEqual("Corrected Game Name", testDataLater.Title);
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

            var testIndex = new SiteDataIndex(logger, string.Empty);
            var builder = new SiteBuilder(logger, new ConfigData(), testIndex);

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            var testPlatformNew = new IgdbPlatform() { Id = 1, Abbreviation = "PSOne" };

            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>() { testPlatformNew });

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

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

            var testIndex = new SiteDataIndex(logger, string.Empty);
            var builder = new SiteBuilder(logger, new ConfigData(), testIndex);

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>());

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

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

            var testIndex = new SiteDataIndex(logger, string.Empty);
            var builder = new SiteBuilder(logger, new ConfigData(), testIndex);

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);
            Assert.AreEqual(0, logger.GetLogs(LogLevel.Warning).Count);

            var testPlatformNew = new IgdbPlatform() { Id = 2, Abbreviation = "PS1" };

            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>() { testPlatformNew });

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline());

            Assert.AreEqual(0, logger.GetLogs(LogLevel.Error).Count);

            var warnings = logger.GetLogs(LogLevel.Warning);
            Assert.AreEqual(1, warnings.Count);

            var expectedMessage = $"Updated data index has a different data ID for PlatformData with key {testPlatformOld.GetReferenceableKey()}: old data ID {testPlatformOld.GetUniqueIdString()} new data ID {testPlatformNew.GetUniqueIdString()}";
            Assert.AreEqual(expectedMessage, warnings[0].Message);
        }
    }
}
