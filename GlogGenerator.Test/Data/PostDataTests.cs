using System.IO;
using GlogGenerator.Data;
using GlogGenerator.MarkdownExtensions;
using Markdig;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test.Data
{
    [TestClass]
    public class PostDataTests
    {
        [TestMethod]
        public void TestFromFilePath()
        {
            var testPostFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "Data", "PostDataTests", "testpostdata.md");

            var builder = new SiteBuilder();
            var testPostData = PostData.MarkdownFromFilePath(builder.GetMarkdownPipeline(), testPostFilePath);

            Assert.AreEqual("2023/07/23/re-climbing-the-orc-chart/", testPostData.PermalinkRelative);
        }

        [TestMethod]
        public void TestToMarkdownStringSimple()
        {
            var testPostFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "Data", "PostDataTests", "testpostdata.md");
            var testPostFileText = File.ReadAllText(testPostFilePath);

            var builder = new SiteBuilder();
            var testPostData = PostData.MarkdownFromFilePath(builder.GetMarkdownPipeline(), testPostFilePath);
            var testPostToString = testPostData.MdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testPostFileText, testPostToString);
        }

        [TestMethod]
        public void TestToMarkdownStringComplex()
        {
            var testPostFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "Data", "PostDataTests", "testfenceddatacharts.md");
            var testPostFileText = File.ReadAllText(testPostFilePath);

            var builder = new SiteBuilder();
            var testPostData = PostData.MarkdownFromFilePath(builder.GetMarkdownPipeline(), testPostFilePath);
            var testPostToString = testPostData.MdDoc.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testPostFileText, testPostToString);
        }
    }
}
