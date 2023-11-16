using System.IO;
using System.Linq;
using System.Text;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Markdig.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tomlyn.Model;

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
someArray = [""an"", ""array""]
+++
hello, world";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());
            var tomlFrontMatter = mdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();

            Assert.IsNotNull(tomlFrontMatter);

            var tomlModel = tomlFrontMatter.GetModel();
            Assert.IsTrue(tomlModel.ContainsKey("someString"));
            Assert.AreEqual("a string value", (string)tomlModel["someString"]);
            Assert.IsTrue(tomlModel.ContainsKey("someArray"));
            Assert.AreEqual(2, ((TomlArray)tomlModel["someArray"]).Count);
            Assert.AreEqual("an", (string)((TomlArray)tomlModel["someArray"])[0]);
            Assert.AreEqual("array", (string)((TomlArray)tomlModel["someArray"])[1]);
        }

        [TestMethod]
        public void TestTomlFrontMatterBlockHtmlRender()
        {
            var builder = new SiteBuilder();

            var testText = @"+++
someString = ""a string value""
someArray = [""an"", ""array""]
+++
hello, world";

            var result = Markdown.ToHtml(testText, builder.GetMarkdownPipeline());

            Assert.AreEqual("<p>hello, world</p>\n", result);
        }

        [TestMethod]
        public void TestTomlFrontMatterBlockRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = @"+++
someString = ""a string value""
someArray = [""an"", ""array""]
+++
hello, world";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testText, result);
        }
    }
}
