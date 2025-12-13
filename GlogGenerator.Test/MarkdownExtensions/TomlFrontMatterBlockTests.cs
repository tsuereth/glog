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
    public class TomlFrontMatterBlockTests
    {
        [TestMethod]
        public void TestTomlFrontMatterBlockParse()
        {
            var builder = new SiteBuilder();

            var testText = @"+++
someString = ""a string value""
someArray = [ ""an"", ""array"" ]
+++
hello, world";

            var mdPipeline = builder.GetContentParser().GetHtmlRenderPipeline();
            var mdDoc = Markdown.Parse(testText, mdPipeline);
            var tomlFrontMatter = mdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();

            Assert.IsNotNull(tomlFrontMatter);

            var tomlModel = tomlFrontMatter.GetModel();
            Assert.IsTrue(tomlModel.HasKey("someString"));
            Assert.AreEqual("a string value", (string)tomlModel["someString"]);
            Assert.IsTrue(tomlModel.HasKey("someArray"));
            Assert.AreEqual(2, ((Tommy.TomlArray)tomlModel["someArray"]).ChildrenCount);
            Assert.AreEqual("an", (string)(tomlModel["someArray"])[0]);
            Assert.AreEqual("array", (string)(tomlModel["someArray"])[1]);
        }

        [TestMethod]
        public void TestTomlFrontMatterBlockHtmlRender()
        {
            var builder = new SiteBuilder();

            var testText = @"+++
someString = ""a string value""
someArray = [ ""an"", ""array"" ]
+++
hello, world";

            var mdPipeline = builder.GetContentParser().GetHtmlRenderPipeline();
            var result = Markdown.ToHtml(testText, mdPipeline);

            Assert.AreEqual("<p>hello, world</p>\n", result);
        }

        [TestMethod]
        public void TestTomlFrontMatterBlockRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = @"+++
someString = ""a string value""
someArray = [ ""an"", ""array"" ]
+++
hello, world";

            var mdPipeline = builder.GetContentParser().GetRoundtripRenderPipeline();
            var mdDoc = Markdown.Parse(testText, mdPipeline);

            var result = mdDoc.ToMarkdownString(mdPipeline);

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestTomlFrontMatterBlockModifiedRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = @"+++
someString = ""a string value""
someArray = [ ""an"", ""array"" ]
+++
hello, world";

            var mdPipeline = builder.GetContentParser().GetRoundtripRenderPipeline();
            var mdDoc = Markdown.Parse(testText, mdPipeline);
            var tomlFrontMatter = mdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();

            Assert.IsNotNull(tomlFrontMatter);

            var tomlModel = tomlFrontMatter.GetModel();
            tomlModel["someString"] = "another string value";
            tomlModel["someArray"][1] = "other";
            tomlModel["someArray"].Add("array");

            var result = mdDoc.ToMarkdownString(mdPipeline);

            var expectedText = @"+++
someString = ""another string value""
someArray = [ ""an"", ""other"", ""array"" ]
+++
hello, world";
            Assert.AreEqual(expectedText, result);
        }
    }
}
