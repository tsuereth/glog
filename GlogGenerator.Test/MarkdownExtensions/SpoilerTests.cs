using System.IO;
using System.Text;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test.MarkdownExtensions
{
    [TestClass]
    public class SpoilerTests
    {
        [TestMethod]
        public void TestSpoiler()
        {
            var builder = new SiteBuilder();

            var testText = "Incoming spoiler: >!ohno spoiler!<";

            var mdPipeline = builder.GetContentParser().GetHtmlRenderPipeline();
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Incoming spoiler: <noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">ohno spoiler</spoiler></p>\n", result);
        }

        [TestMethod]
        public void TestSpoilerIntoNextParagraph()
        {
            var builder = new SiteBuilder();

            var testText = "This is a poorly-formatted spoiler: >!forgot to close it\n\nBut it just keeps going";

            var mdPipeline = builder.GetContentParser().GetHtmlRenderPipeline();
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>This is a poorly-formatted spoiler: <noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">forgot to close it</p>\n<p>But it just keeps going</p>\n", result);
        }

        [TestMethod]
        public void TestSpoilerWithMisleadingCloser()
        {
            var builder = new SiteBuilder();

            var testText = "Exciting spoiler: >!super duper <i>spoiler!</i>!<";

            var mdPipeline = builder.GetContentParser().GetHtmlRenderPipeline();
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Exciting spoiler: <noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">super duper <i>spoiler!</i></spoiler></p>\n", result);
        }

        [TestMethod]
        public void TestSpoilerNested()
        {
            var builder = new SiteBuilder();

            var testText = "Incoming spoiler: >!ohno >!spoiler within a!< spoiler!<";

            var mdPipeline = builder.GetContentParser().GetHtmlRenderPipeline();
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Incoming spoiler: <noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">ohno <noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">spoiler within a</spoiler> spoiler</spoiler></p>\n", result);
        }

        [TestMethod]
        public void TestSpoilerNormalize()
        {
            var builder = new SiteBuilder();

            var testText = "Incoming spoiler: >!ohno spoiler!<";

            var mdPipeline = builder.GetContentParser().GetRoundtripRenderPipeline();
            var result = Markdown.Normalize(testText, pipeline: mdPipeline);

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestSpoilerRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = "Incoming spoiler: >!ohno spoiler!<";

            var mdPipeline = builder.GetContentParser().GetRoundtripRenderPipeline();
            var mdDoc = Markdown.Parse(testText, mdPipeline);

            var result = mdDoc.ToMarkdownString(mdPipeline);

            Assert.AreEqual(testText, result);
        }
    }
}
