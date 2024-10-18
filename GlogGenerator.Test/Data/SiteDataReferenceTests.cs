using System;
using GlogGenerator.Data;
using GlogGenerator.IgdbApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace GlogGenerator.Test.Data
{
    [TestClass]
    public class SiteDataReferenceTests
    {
        [TestMethod]
        public void TestNewUnresolvedReference()
        {
            var testReference = new SiteDataReference<TagData>("Test Tag");

            Assert.IsFalse(testReference.GetIsResolved());
        }

        [TestMethod]
        public void TestResolvedReference()
        {
            var testTag = new TagData(typeof(IgdbEntity), "Test Tag");

            var testReference = new SiteDataReference<TagData>("Test Tag");
            testReference.SetData(testTag);

            Assert.IsTrue(testReference.GetIsResolved());
            Assert.IsTrue(string.IsNullOrEmpty(testReference.GetUnresolvedReferenceKey()));

            Assert.AreEqual(testTag.GetDataId(), testReference.GetResolvedReferenceId());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestResolveEmptyIdException()
        {
            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns(string.Empty);

            var testReference = new SiteDataReference<IGlogReferenceable>("Test Tag");
            testReference.SetData(mockData);
        }
    }
}
