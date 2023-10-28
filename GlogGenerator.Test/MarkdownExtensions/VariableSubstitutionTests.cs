using System.IO;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test.MarkdownExtensions
{
    [TestClass]
    public class VariableSubstitutionTests
    {
        private static MarkdownPipeline CreateMdPipeline(SiteConfig siteConfig)
        {
            var mdPipeline = new MarkdownPipelineBuilder()
                .UseGenericAttributes()
                .UseMediaLinks()
                .Use(new GlogMarkdownExtension(siteConfig, null, null))
                .Build();

            return mdPipeline;
        }

        [TestMethod]
        public void TestInlineSubstitution()
        {
            var config = new SiteConfig();
            config.GetVariableSubstitution().SetSubstitution("TestVar", "replacement text");

            var testText = "Test of $TestVar$ substitution";

            var mdPipeline = CreateMdPipeline(config);
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Test of replacement text substitution</p>\n", result);
        }

        [TestMethod]
        public void TestInlineVariableNotFound()
        {
            var config = new SiteConfig();
            config.GetVariableSubstitution().SetSubstitution("TestVar", "replacement text");

            var testText = "Test of $TestVart$ substitution";

            var mdPipeline = CreateMdPipeline(config);
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Test of $TestVart$ substitution</p>\n", result);
        }

        [TestMethod]
        public void TestInlineVariableAfterStrayToken()
        {
            var config = new SiteConfig();
            config.GetVariableSubstitution().SetSubstitution("TestVar", "replacement text");

            var testText = "Test $of $TestVar$ substitution";

            var mdPipeline = CreateMdPipeline(config);
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Test $of replacement text substitution</p>\n", result);
        }

        [TestMethod]
        public void TestAutolinkInlineSubstitution()
        {
            var config = new SiteConfig();
            config.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of <https://$TestVar$> substitution";

            var mdPipeline = CreateMdPipeline(config);
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Test of <a href=\"https://replacement.text\">https://replacement.text</a> substitution</p>\n", result);
        }

        [TestMethod]
        public void TestLinkInlineSubstitution()
        {
            var config = new SiteConfig();
            config.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of [link text](https://$TestVar$) substitution";

            var mdPipeline = CreateMdPipeline(config);
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Test of <a href=\"https://replacement.text\">link text</a> substitution</p>\n", result);
        }

        [TestMethod]
        public void TestMediaLinkSubstitution()
        {
            var config = new SiteConfig();
            config.GetVariableSubstitution().SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of ![static mp4](https://$TestVar$/video.mp4){width=960 height=540 controls} substitution";

            var mdPipeline = CreateMdPipeline(config);
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Test of <video width=\"960\" height=\"540\" controls=\"\"><source type=\"video/mp4\" src=\"https://replacement.text/video.mp4\"></source></video> substitution</p>\n", result);
        }
    }
}
