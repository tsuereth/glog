using System.IO;
using System.Text;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Markdig.Renderers.Normalize;
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

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Incoming spoiler: <noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">ohno spoiler</spoiler></p>\n", result);
        }

        [TestMethod]
        public void TestSpoilerIntoNextParagraph()
        {
            var builder = new SiteBuilder();

            var testText = "This is a poorly-formatted spoiler: >!forgot to close it\n\nBut it just keeps going";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>This is a poorly-formatted spoiler: <noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">forgot to close it</p>\n<p>But it just keeps going</p>\n", result);
        }

        [TestMethod]
        public void TestSpoilerWithMisleadingCloser()
        {
            var builder = new SiteBuilder();

            var testText = "Exciting spoiler: >!super duper <i>spoiler!</i>!<";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Exciting spoiler: <noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">super duper <i>spoiler!</i></spoiler></p>\n", result);
        }

        [TestMethod]
        public void TestSpoilerNested()
        {
            var builder = new SiteBuilder();

            var testText = "Incoming spoiler: >!ohno >!spoiler within a!< spoiler!<";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Incoming spoiler: <noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">ohno <noscript><i>JavaScript is disabled, and this concealed spoiler may not appear as expected.</i></noscript><spoiler class=\"spoiler_hidden\" onClick=\"spoiler_toggle(this);\">spoiler within a</spoiler> spoiler</spoiler></p>\n", result);
        }

        [Ignore]
        [TestMethod]
        public void TestSpoilerNormalized()
        {
            var builder = new SiteBuilder();

            var testText = "Incoming spoiler: >!ohno spoiler!<";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            string normalized;
            using (var mdTextWriter = new StringWriter())
            {
                var mdRenderer = new NormalizeRenderer(mdTextWriter);
                mdRenderer.Render(mdDoc);

                normalized = mdTextWriter.ToString();
            }

            Assert.AreEqual(testText, normalized);
        }
    }
}
