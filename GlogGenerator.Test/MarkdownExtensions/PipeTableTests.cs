using GlogGenerator.MarkdownExtensions;
using Markdig;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test.MarkdownExtensions
{
    [TestClass]
    public class PipeTableTests
    {
        [TestMethod]
        public void TestTableOneRowRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = @"| ![](img1.png){width=200 height=127} | ![](img2.png){width=586 height=251} |
| - | - |";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownRoundtripPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownRoundtripPipeline());

            Assert.AreEqual(testText, result);
        }

        [TestMethod]
        public void TestTableManyRowsRoundtrip()
        {
            var builder = new SiteBuilder();

            var testText = @"| ![](img1.jpg){width=200 height=150} | ![](img2.jpg){width=200 height=150} | ![](img3.jpg){width=200 height=150} |
| - | - | - |
| ![](img4.jpg){width=200 height=150} | ![](img5.jpg){width=200 height=150} | ![](img6.jpg){width=200 height=150} |
| ![](img7.jpg){width=200 height=150} | ![](img8.jpg){width=200 height=150} | ![](img9.jpg){width=200 height=150} |";

            var mdDoc = Markdown.Parse(testText, builder.GetMarkdownRoundtripPipeline());

            var result = mdDoc.ToMarkdownString(builder.GetMarkdownRoundtripPipeline());

            Assert.AreEqual(testText, result);
        }
    }
}
