using System.IO;
using System.Text;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test.MarkdownExtensions
{
    [TestClass]
    public class VariableSubstitutionTests
    {
        [TestMethod]
        public void TestInlineSubstitution()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement text");

            var testText = "Test of $TestVar$ substitution";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Test of replacement text substitution</p>\n", result);
        }

        [TestMethod]
        public void TestInlineVariableNotFound()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement text");

            var testText = "Test of $TestVart$ substitution";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Test of $TestVart$ substitution</p>\n", result);
        }

        [TestMethod]
        public void TestInlineVariableAfterStrayToken()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement text");

            var testText = "Test $of $TestVar$ substitution";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Test $of replacement text substitution</p>\n", result);
        }

        [TestMethod]
        public void TestInlineSubstitutionNormalize()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement text");

            var testText = "Test of $TestVar$ substitution";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestInlineSubstitutionRoundtrip()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement text");

            var testText = "Test of $TestVar$ substitution";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestAutolinkInlineSubstitution()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of <https://$TestVar$> substitution";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Test of <a href=\"https://replacement.text\">https://replacement.text</a> substitution</p>\n", result);
        }

        [TestMethod]
        public void TestAutolinkInlineSubstitutionNormalize()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of <https://$TestVar$> substitution";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestAutolinkInlineSubstitutionRoundtrip()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of <https://$TestVar$> substitution";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestLinkInlineSubstitution()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of [link text](https://$TestVar$) substitution";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Test of <a href=\"https://replacement.text\">link text</a> substitution</p>\n", result);
        }

        [TestMethod]
        public void TestLinkInlineSubstitutionNormalize()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of [link text](https://$TestVar$) substitution";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestLinkInlineSubstitutionRoundtrip()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of [link text](https://$TestVar$) substitution";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestMediaLinkSubstitution()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of ![static mp4](https://$TestVar$/video.mp4){width=960 height=540 controls} substitution";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>Test of <video width=\"960\" height=\"540\" controls=\"\"><source type=\"video/mp4\" src=\"https://replacement.text/video.mp4\"></source></video> substitution</p>\n", result);
        }

        [TestMethod]
        public void TestMediaLinkSubstitutionNormalize()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of ![static mp4](https://$TestVar$/video.mp4){width=960 height=540 controls} substitution";

            var result = Markdown.Normalize(testText, pipeline: builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestMediaLinkSubstitutionRoundtrip()
        {
            var builder = new SiteBuilder();
            builder.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of ![static mp4](https://$TestVar$/video.mp4){width=960 height=540 controls} substitution";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }
    }
}
