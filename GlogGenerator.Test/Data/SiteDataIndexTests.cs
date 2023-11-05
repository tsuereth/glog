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
            using var loggerFactory = LoggerFactory.Create(c => c.AddConsole());

            var logger = loggerFactory.CreateLogger<Program>();

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

            var testTag = testIndex.GetTag("Sonic the Hedgehog");
            Assert.AreEqual("Sonic The Hedgehog", testTag.Name);
        }
    }
}
