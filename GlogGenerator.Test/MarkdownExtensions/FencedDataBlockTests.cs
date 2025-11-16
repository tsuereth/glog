using System.IO;
using System.Linq;
using System.Text;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Markdig.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test.MarkdownExtensions
{
    [TestClass]
    public class FencedDataBlockTests
    {
        [TestMethod]
        public void TestFencedDataBlockParse()
        {
            var builder = new SiteBuilder();

            var testText = @":::testitem
key1: ""value1""
key2: ""value2""
:::";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownHtmlPipeline());
            var fencedDataBlock = mdDoc.Descendants<FencedDataBlock>().FirstOrDefault();

            Assert.IsNotNull(fencedDataBlock);

            var parsedData = fencedDataBlock.Data;
            Assert.IsTrue(parsedData.ContainsKey("key1"));
            Assert.AreEqual("\"value1\"", parsedData["key1"]);
            Assert.IsTrue(parsedData.ContainsKey("key2"));
            Assert.AreEqual("\"value2\"", parsedData["key2"]);
        }

        // TODO?: test HTML rendering

        [TestMethod]
        public void TestFencedDataBlockNormalize()
        {
            var builder = new SiteBuilder();

            var testText = @":::testitem
key1: ""value1""
key2: ""value2""
:::";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownRoundtripPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestFencedDataBlockRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = @":::testitem
key1: ""value1""
key2: ""value2""
:::";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownRoundtripPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownRoundtripPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestFencedDataBlockTwoInARowRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = @":::testitem
key1: ""value1""
key2: ""value2""
:::

:::testitem
key1: ""value1""
key2: ""value2""
:::";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownRoundtripPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownRoundtripPipeline());

            Assert.AreEqual(testText, result);
        }
    }
}
