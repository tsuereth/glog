using GlogGenerator.DiffSummary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test.DiffSummary
{
    [TestClass]
    public class DiffFilePathsTests
    {
        [TestMethod]
        public void TestGetBasePathsOneFilePath()
        {
            var testPaths = new DiffFilePaths("subdir/file.txt");

            var result = testPaths.GetBasePaths();
            Assert.HasCount(1, result);
            Assert.AreEqual("subdir/", result[0]);
        }

        [TestMethod]
        public void TestGetBasePathsOneFilePathNoSubdirs()
        {
            var testPaths = new DiffFilePaths("file.txt");

            var result = testPaths.GetBasePaths();
            Assert.HasCount(1, result);
            Assert.AreEqual("file.txt", result[0]);
        }

        [TestMethod]
        public void TestGetBasePathsManyFilePaths()
        {
            var testPaths = new DiffFilePaths("subdir/file1.txt");
            testPaths.Add("subdir/file2.txt");
            testPaths.Add("subdir/moresubdir/file3.txt");

            var result = testPaths.GetBasePaths();
            Assert.HasCount(1, result);
            Assert.AreEqual("subdir/", result[0]);
        }

        [TestMethod]
        public void TestGetBasePathsNoCommonBasePath()
        {
            var testPaths = new DiffFilePaths("subdir/file1.txt");
            testPaths.Add("subdir/file2.txt");
            testPaths.Add("differentsubdir/moresubdir/file3.txt");

            var result = testPaths.GetBasePaths();
            Assert.HasCount(2, result);
            Assert.AreEqual("differentsubdir/", result[0]);
            Assert.AreEqual("subdir/", result[1]);
        }

        [TestMethod]
        public void TestGetBasePathsNoCommonBasePathNoSubdirs()
        {
            var testPaths = new DiffFilePaths("file1.txt");
            testPaths.Add("file2.txt");
            testPaths.Add("file3.txt");

            var result = testPaths.GetBasePaths();
            Assert.HasCount(3, result);
            Assert.AreEqual("file1.txt", result[0]);
            Assert.AreEqual("file2.txt", result[1]);
            Assert.AreEqual("file3.txt", result[2]);
        }
    }
}
