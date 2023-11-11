using System.IO;
using GlogGenerator.Data;
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

        [Ignore]
        [TestMethod]
        public void TestToMarkdownString()
        {
            var testPostFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "Data", "PostDataTests", "testfenceddatacharts.md");
            var testPostFileText = File.ReadAllText(testPostFilePath);

            var builder = new SiteBuilder();
            var testPostData = PostData.MarkdownFromFilePath(builder.GetMarkdownPipeline(), testPostFilePath);
            var testPostToString = testPostData.ToMarkdownString(builder.GetMarkdownPipeline());

            Assert.AreEqual(testPostFileText, testPostToString);
        }
    }
}
