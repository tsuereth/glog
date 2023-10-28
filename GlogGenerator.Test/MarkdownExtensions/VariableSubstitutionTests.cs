using System.IO;
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
            var vs = new VariableSubstitution();
            vs.SetSubstitution("TestVar", "replacement text");

            var testText = "Test of $TestVar$ substitution";

            var mdPipeline = new MarkdownPipelineBuilder()
                .Use(new GlogMarkdownExtension(null, null, null, vs))
                .Build();
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Test of replacement text substitution</p>\n", result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void TestInlineVariableNotFound()
        {
            var vs = new VariableSubstitution();
            vs.SetSubstitution("TestVar", "replacement text");

            var testText = "Test of $TestVart$ substitution";

            var mdPipeline = new MarkdownPipelineBuilder()
                .Use(new GlogMarkdownExtension(null, null, null, vs))
                .Build();
            var result = Markdown.ToHtml(testText, mdPipeline);
        }

        [TestMethod]
        public void TestAutolinkInlineSubstitution()
        {
            var vs = new VariableSubstitution();
            vs.SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of <https://$TestVar$> substitution";

            var mdPipeline = new MarkdownPipelineBuilder()
                .Use(new GlogMarkdownExtension(null, null, null, vs))
                .Build();
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Test of <a href=\"https://replacement.text\">https://replacement.text</a> substitution</p>\n", result);
        }

        [TestMethod]
        public void TestLinkInlineSubstitution()
        {
            var vs = new VariableSubstitution();
            vs.SetSubstitution("TestVar", "replacement.text");

            var testText = "Test of [link text](https://$TestVar$) substitution";

            var mdPipeline = new MarkdownPipelineBuilder()
                .Use(new GlogMarkdownExtension(null, null, null, vs))
                .Build();
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>Test of <a href=\"https://replacement.text\">link text</a> substitution</p>\n", result);
        }
    }
}
