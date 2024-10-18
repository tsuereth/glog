using System;
using System.Collections.Generic;
using System.Linq;
using GlogGenerator.Data;
using GlogGenerator.IgdbApi;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace GlogGenerator.Test.MarkdownExtensions
{
    [TestClass]
    public class GlogLinkTests
    {
        private static SiteBuilder PrepareTestSiteBuilder(
            params IGlogReferenceable[] dataItems)
        {
            var logger = new TestLogger();

            var configData = new ConfigData()
            {
                BaseURL = "fake://test.url/",
            };

            var mockSiteDataIndex = Substitute.For<ISiteDataIndex>();
            var testGames = new List<GameData>();
            var testPlatforms = new List<PlatformData>();
            var testTags = new List<TagData>();
            foreach (var dataItem in dataItems)
            {
                if (dataItem is GameData)
                {
                    var gameData = dataItem as GameData;
                    mockSiteDataIndex.GetGame(gameData.Title).Returns(gameData);
                    testGames.Add(gameData);
                }
                else if (dataItem is PlatformData)
                {
                    var platformData = dataItem as PlatformData;
                    mockSiteDataIndex.GetPlatform(platformData.Abbreviation).Returns(platformData);
                    testPlatforms.Add(platformData);
                }
                else if (dataItem is TagData)
                {
                    var tagData = dataItem as TagData;
                    mockSiteDataIndex.GetTag(tagData.Name).Returns(tagData);
                    testTags.Add(tagData);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            mockSiteDataIndex.CreateReference<GameData>(Arg.Any<string>()).Returns(args =>
            {
                var referenceKey = args.ArgAt<string>(0);
                return new SiteDataReference<GameData>(referenceKey);
            });

            mockSiteDataIndex.CreateReference<PlatformData>(Arg.Any<string>()).Returns(args =>
            {
                var referenceKey = args.ArgAt<string>(0);
                return new SiteDataReference<PlatformData>(referenceKey);
            });

            mockSiteDataIndex.CreateReference<TagData>(Arg.Any<string>()).Returns(args =>
            {
                var referenceKey = args.ArgAt<string>(0);
                return new SiteDataReference<TagData>(referenceKey);
            });

            mockSiteDataIndex.GetData<GameData>(Arg.Any<SiteDataReference<GameData>>()).Returns(args =>
            {
                var dataReference = args.ArgAt<SiteDataReference<GameData>>(0);
                var key = dataReference.GetUnresolvedReferenceKey();
                var data = testGames.Where(g => g.MatchesReferenceableKey(key)).FirstOrDefault();
                return data;
            });

            mockSiteDataIndex.GetData<PlatformData>(Arg.Any<SiteDataReference<PlatformData>>()).Returns(args =>
            {
                var dataReference = args.ArgAt<SiteDataReference<PlatformData>>(0);
                var key = dataReference.GetUnresolvedReferenceKey();
                var data = testPlatforms.Where(g => g.MatchesReferenceableKey(key)).FirstOrDefault();
                return data;
            });

            mockSiteDataIndex.GetData<TagData>(Arg.Any<SiteDataReference<TagData>>()).Returns(args =>
            {
                var dataReference = args.ArgAt<SiteDataReference<TagData>>(0);
                var key = dataReference.GetUnresolvedReferenceKey();
                var data = testTags.Where(g => g.MatchesReferenceableKey(key)).FirstOrDefault();
                return data;
            });

            mockSiteDataIndex.GetGames().Returns(testGames);
            mockSiteDataIndex.GetPlatforms().Returns(testPlatforms);
            mockSiteDataIndex.GetTags().Returns(testTags);

            var builder = new SiteBuilder(
                logger,
                configData,
                mockSiteDataIndex);

            builder.UpdateContentRoutes();

            return builder;
        }

        [TestMethod]
        public void TestGlogGameAutolink()
        {
            var testGameData = new GameData()
            {
                Title = "Test Game: With a Subtitle",
            };

            var builder = PrepareTestSiteBuilder(
                testGameData);

            var testText = "Remember <game:Test Game: With a Subtitle>?";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Remember <a href=\"fake://test.url/game/test-game-with-a-subtitle\">Test Game: With a Subtitle</a>?</p>\n", result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGlogGameAutolinkNotFound()
        {
            var builder = PrepareTestSiteBuilder();

            var testText = "Remember <game:Test Game: With a Subtitle>?";

            Markdown.ToHtml(testText, builder.GetMarkdownPipeline());
        }

        [TestMethod]
        public void TestGlogGameAutolinkNormalize()
        {
            var testGameData = new GameData()
            {
                Title = "Test Game: With a Subtitle",
            };

            var builder = PrepareTestSiteBuilder(
                testGameData);

            var testText = "Remember <game:Test Game: With a Subtitle>?";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogGameAutolinkRoundtrip()
        {
            var testGameData = new GameData()
            {
                Title = "Test Game: With a Subtitle",
            };

            var builder = PrepareTestSiteBuilder(
                testGameData);

            var testText = "Remember <game:Test Game: With a Subtitle>?";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogGameLink()
        {
            var testGameData = new GameData()
            {
                Title = "Test Game: With a Subtitle",
            };

            var builder = PrepareTestSiteBuilder(
                testGameData);

            var testText = "Remember [that game with a stupid subtitle](game:Test Game: With a Subtitle)?";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Remember <a href=\"fake://test.url/game/test-game-with-a-subtitle\">that game with a stupid subtitle</a>?</p>\n", result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGlogGameLinkNotFound()
        {
            var builder = PrepareTestSiteBuilder();

            var testText = "Remember [that game with a stupid subtitle](game:Test Game: With a Subtitle)?";

            Markdown.ToHtml(testText, builder.GetMarkdownPipeline());
        }

        [TestMethod]
        public void TestGlogGameLinkNormalize()
        {
            var testGameData = new GameData()
            {
                Title = "Test Game: With a Subtitle",
            };

            var builder = PrepareTestSiteBuilder(
                testGameData);

            var testText = "Remember [that game with a stupid subtitle](game:Test Game: With a Subtitle)?";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogGameLinkRoundtrip()
        {
            var testGameData = new GameData()
            {
                Title = "Test Game: With a Subtitle",
            };

            var builder = PrepareTestSiteBuilder(
                testGameData);

            var testText = "Remember [that game with a stupid subtitle](game:Test Game: With a Subtitle)?";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogPlatformAutolink()
        {
            var testPlatformData = new PlatformData()
            {
                Abbreviation = "GS2",
                Name = "GlogStation 2",
            };

            var builder = PrepareTestSiteBuilder(
                testPlatformData);

            var testText = "There were no good <platform:GS2> games";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>There were no good <a href=\"fake://test.url/platform/gs2\">GS2</a> games</p>\n", result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGlogPlatformAutolinkNotFound()
        {
            var builder = PrepareTestSiteBuilder();

            var testText = "There were no good <platform:GS2> games";

            Markdown.ToHtml(testText, builder.GetMarkdownPipeline());
        }

        [TestMethod]
        public void TestGlogPlatformAutolinkNormalize()
        {
            var testPlatformData = new PlatformData()
            {
                Abbreviation = "GS2",
                Name = "GlogStation 2",
            };

            var builder = PrepareTestSiteBuilder(
                testPlatformData);

            var testText = "There were no good <platform:GS2> games";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogPlatformAutolinkRoundtrip()
        {
            var testPlatformData = new PlatformData()
            {
                Abbreviation = "GS2",
                Name = "GlogStation 2",
            };

            var builder = PrepareTestSiteBuilder(
                testPlatformData);

            var testText = "There were no good <platform:GS2> games";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogPlatformLink()
        {
            var testPlatformData = new PlatformData()
            {
                Abbreviation = "GS2",
                Name = "GlogStation 2",
            };

            var builder = PrepareTestSiteBuilder(
                testPlatformData);

            var testText = "There were no good [GlogStation 2](platform:GS2) games";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>There were no good <a href=\"fake://test.url/platform/gs2\">GlogStation 2</a> games</p>\n", result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGlogPlatformLinkNotFound()
        {
            var builder = PrepareTestSiteBuilder();

            var testText = "There were no good [GlogStation 2](platform:GS2) games";

            Markdown.ToHtml(testText, builder.GetMarkdownPipeline());
        }

        [TestMethod]
        public void TestGlogPlatformLinkNormalize()
        {
            var testPlatformData = new PlatformData()
            {
                Abbreviation = "GS2",
                Name = "GlogStation 2",
            };

            var builder = PrepareTestSiteBuilder(
                testPlatformData);

            var testText = "There were no good [GlogStation 2](platform:GS2) games";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogPlatformLinkRoundtrip()
        {
            var testPlatformData = new PlatformData()
            {
                Abbreviation = "GS2",
                Name = "GlogStation 2",
            };

            var builder = PrepareTestSiteBuilder(
                testPlatformData);

            var testText = "There were no good [GlogStation 2](platform:GS2) games";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogTagAutolink()
        {
            var testTagData = new TagData(typeof(IgdbEntity), "Gamedev Inc.");

            var builder = PrepareTestSiteBuilder(
                testTagData);

            var testText = "Waiting for <tag:Gamedev Inc.>'s next game";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Waiting for <a href=\"fake://test.url/tag/gamedev-inc\">Gamedev Inc.</a>'s next game</p>\n", result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGlogTagAutolinkNotFound()
        {
            var builder = PrepareTestSiteBuilder();

            var testText = "Waiting for <tag:Gamedev Inc.>'s next game";

            Markdown.ToHtml(testText, builder.GetMarkdownPipeline());
        }

        [TestMethod]
        public void TestGlogTagAutolinkNormalize()
        {
            var testTagData = new TagData(typeof(IgdbEntity), "Gamedev Inc.");

            var builder = PrepareTestSiteBuilder(
                testTagData);

            var testText = "Waiting for <tag:Gamedev Inc.>'s next game";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogTagAutolinkRoundtrip()
        {
            var testTagData = new TagData(typeof(IgdbEntity), "Gamedev Inc.");

            var builder = PrepareTestSiteBuilder(
                testTagData);

            var testText = "Waiting for <tag:Gamedev Inc.>'s next game";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogTagLink()
        {
            var testTagData = new TagData(typeof(IgdbEntity), "Gamedev Inc.");

            var builder = PrepareTestSiteBuilder(
                testTagData);

            var testText = "Waiting for [that studio](tag:Gamedev Inc.)'s next game";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Waiting for <a href=\"fake://test.url/tag/gamedev-inc\">that studio</a>'s next game</p>\n", result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGlogTagLinkNotFound()
        {
            var builder = PrepareTestSiteBuilder();

            var testText = "Waiting for [that studio](tag:Gamedev Inc.)'s next game";

            Markdown.ToHtml(testText, builder.GetMarkdownPipeline());
        }

        [TestMethod]
        public void TestGlogTagLinkNormalize()
        {
            var testTagData = new TagData(typeof(IgdbEntity), "Gamedev Inc.");

            var builder = PrepareTestSiteBuilder(
                testTagData);

            var testText = "Waiting for [that studio](tag:Gamedev Inc.)'s next game";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestGlogTagLinkRoundtrip()
        {
            var testTagData = new TagData(typeof(IgdbEntity), "Gamedev Inc.");

            var builder = PrepareTestSiteBuilder(
                testTagData);

            var testText = "Waiting for [that studio](tag:Gamedev Inc.)'s next game";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }
    }
}
