using System;
using System.Collections.Generic;
using System.Linq;
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
        private static bool IsDefaultTagData(TagData tagData)
        {
            var igdbGameCategories = (IgdbGameCategory[])Enum.GetValues(typeof(IgdbGameCategory));
            var igdbGameCategoryDescriptions = igdbGameCategories.Where(c => c != IgdbGameCategory.None).Select(c => c.Description());
            if (igdbGameCategoryDescriptions.Contains(tagData.Name))
            {
                return true;
            }

            return false;
        }

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

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Error));
            Assert.IsEmpty(logger.GetLogs(LogLevel.Warning));

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

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Error));
            Assert.IsEmpty(logger.GetLogs(LogLevel.Warning));

            var testReference = testIndex.CreateReference<GameData>(testDataItem.Name, true);
            var testData = testIndex.GetData(testReference);

            Assert.IsTrue(testReference.GetIsResolved());
            Assert.AreEqual("Some Game Name", testData.Title);

            testDataItem.Name = "Corrected Game Name";

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            var errors = logger.GetLogs(LogLevel.Error);
            Assert.HasCount(1, errors);

            var expectedMessage = $"Updated data index has a different key for GameData with data ID {testDataItem.GetUniqueIdString(mockIgdbCache)}: old key Some Game Name new key Corrected Game Name";
            Assert.AreEqual(expectedMessage, errors[0].Message);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Warning));

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

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Error));
            Assert.IsEmpty(logger.GetLogs(LogLevel.Warning));

            testIndex.CreateReference<PlatformData>(testPlatformOld.Abbreviation, true);
            testIndex.ResolveReferences();

            var testPlatformNew = new IgdbPlatform() { Id = 1, Abbreviation = "PSOne" };

            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>() { testPlatformNew });

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            var errors = logger.GetLogs(LogLevel.Error);
            Assert.HasCount(1, errors);

            var expectedMessage = $"Updated data index has a different key for PlatformData with data ID {testPlatformOld.GetUniqueIdString(mockIgdbCache)}: old key {testPlatformOld.GetReferenceString(mockIgdbCache)} new key {testPlatformNew.GetReferenceString(mockIgdbCache)}";
            Assert.AreEqual(expectedMessage, errors[0].Message);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Warning));
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

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Error));
            Assert.IsEmpty(logger.GetLogs(LogLevel.Warning));

            testIndex.CreateReference<PlatformData>(testPlatformOld.Abbreviation, true);
            testIndex.ResolveReferences();

            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>());

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            var errors = logger.GetLogs(LogLevel.Error);
            Assert.HasCount(1, errors);

            var expectedMessage = $"Updated data index is missing old PlatformData with data ID {testPlatformOld.GetUniqueIdString(mockIgdbCache)} and key {testPlatformOld.GetReferenceString(mockIgdbCache)}";
            Assert.AreEqual(expectedMessage, errors[0].Message);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Warning));
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

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Error));
            Assert.IsEmpty(logger.GetLogs(LogLevel.Warning));

            testIndex.CreateReference<PlatformData>(testPlatformOld.Abbreviation, true);
            testIndex.ResolveReferences();

            var testPlatformNew = new IgdbPlatform() { Id = 2, Abbreviation = testPlatformOld.Abbreviation };

            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>() { testPlatformNew });

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Error));

            var warnings = logger.GetLogs(LogLevel.Warning);
            Assert.HasCount(1, warnings);

            var expectedMessage = $"Updated data index has a different data ID for PlatformData with key {testPlatformOld.GetReferenceString(mockIgdbCache)}: old data ID {testPlatformOld.GetUniqueIdString(mockIgdbCache)} new data ID {testPlatformNew.GetUniqueIdString(mockIgdbCache)}";
            Assert.AreEqual(expectedMessage, warnings[0].Message);
        }

        [TestMethod]
        public void TestLoadContentUpdateNonContentReferenceChanged()
        {
            var logger = new TestLogger();

            var testCompany = new IgdbCompany() { Id = 1234, Name = "The Developer" };
            var testInvolvedCompany = new IgdbInvolvedCompany() { Id = 5678, CompanyId = testCompany.Id };
            var testGame = new IgdbGame()
            {
                Id = 1,
                Name = "Video Game",
                InvolvedCompanyIds = new List<int>() { testInvolvedCompany.Id },
            };

            var mockIgdbCache = Substitute.For<IIgdbCache>();
            mockIgdbCache.GetAllGames().Returns(new List<IgdbGame>() { testGame });
            mockIgdbCache.GetAllPlatforms().Returns(new List<IgdbPlatform>());
            mockIgdbCache.GetAllGameMetadata().Returns(new List<IgdbEntity>() { testCompany });

            var testIndex = new SiteDataIndex(logger, string.Empty);
            var builder = new SiteBuilder(logger, new ConfigData(), testIndex);

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Error));
            Assert.IsEmpty(logger.GetLogs(LogLevel.Warning));

            var indexTags = testIndex.GetTags().Where(t => !IsDefaultTagData(t)).ToList();
            Assert.HasCount(1, indexTags);
            Assert.AreEqual(testCompany.Name, indexTags[0].Name);

            var testCompanyUpdated = new IgdbCompany() { Id = testCompany.Id, Name = "The Updated Developer" };

            mockIgdbCache.GetAllGameMetadata().Returns(new List<IgdbEntity>() { testCompanyUpdated });

            testIndex.LoadContent(mockIgdbCache, builder.GetMarkdownPipeline(), includeDrafts: false);

            Assert.IsEmpty(logger.GetLogs(LogLevel.Error));
            Assert.IsEmpty(logger.GetLogs(LogLevel.Warning));

            indexTags = testIndex.GetTags().Where(t => !IsDefaultTagData(t)).ToList();
            Assert.HasCount(1, indexTags);
            Assert.AreEqual(testCompanyUpdated.Name, indexTags[0].Name);
        }
    }
}
