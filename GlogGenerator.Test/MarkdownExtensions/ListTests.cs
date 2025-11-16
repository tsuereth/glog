using GlogGenerator.MarkdownExtensions;
using Markdig;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test.MarkdownExtensions
{
    [TestClass]
    public class ListTests
    {
        [TestMethod]
        public void TestUnorderedList()
        {
            var builder = new SiteBuilder();

            var testText = @"* first item
* second item
* third item";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownHtmlPipeline());

            Assert.AreEqual("<ul>\n<li>first item</li>\n<li>second item</li>\n<li>third item</li>\n</ul>\n", result);
        }

        [TestMethod]
        public void TestUnorderedListRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = @"* first item
* second item
* third item";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownRoundtripPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownRoundtripPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestOrderedListDigits()
        {
            var builder = new SiteBuilder();

            var testText = @"1. first item
2. second item
3. third item";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownHtmlPipeline());

            Assert.AreEqual("<ol>\n<li>first item</li>\n<li>second item</li>\n<li>third item</li>\n</ol>\n", result);
        }

        [TestMethod]
        public void TestOrderedListDigitsRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = @"1. first item
2. second item
3. third item";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownRoundtripPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownRoundtripPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestOrderedListLetters()
        {
            var builder = new SiteBuilder();

            var testText = @"A. first item
B. second item
C. third item";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownHtmlPipeline());

            Assert.AreEqual("<ol type=\"A\">\n<li>first item</li>\n<li>second item</li>\n<li>third item</li>\n</ol>\n", result);
        }

        [TestMethod]
        public void TestOrderedListLettersRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = @"A. first item
B. second item
C. third item";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownRoundtripPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownRoundtripPipeline());

            Assert.AreEqual(testText, result);
        }
    }
}
