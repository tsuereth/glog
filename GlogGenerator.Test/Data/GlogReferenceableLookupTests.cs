using System;
using System.Linq;
using GlogGenerator.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace GlogGenerator.Test.Data
{
    [TestClass]
    public class GlogReferenceableLookupTests
    {
        [TestMethod]
        public void TestAddData()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);
        }

        [TestMethod]
        public void TestAddDataEmptyId()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns(string.Empty);
            mockData.GetReferenceableKey().Returns("testkey");

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                testLookup.AddData(mockData);
            });
        }

        [TestMethod]
        public void TestAddDataEmptyReferenceableKey()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns(string.Empty);

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                testLookup.AddData(mockData);
            });
        }

        [TestMethod]
        public void TestAddDataDuplicateId()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            var mockDataDuplicate = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey2");

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                testLookup.AddData(mockDataDuplicate);
            });
        }

        [TestMethod]
        public void TestAddDataDuplicateReferenceableKey()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            var mockDataDuplicate = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid2");
            mockData.GetReferenceableKey().Returns("testkey");

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                testLookup.AddData(mockDataDuplicate);
            });
        }

        [TestMethod]
        public void TestRemoveDataById()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            Assert.HasCount(1, testLookup.GetValues());

            testLookup.RemoveDataById("testid");

            Assert.HasCount(0, testLookup.GetValues());
        }

        [TestMethod]
        public void TestRemoveDataByIdNotFound()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                testLookup.RemoveDataById("testid");
            });
        }

        [TestMethod]
        public void TestRemoveDataByReferenceableKey()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            Assert.HasCount(1, testLookup.GetValues());

            testLookup.RemoveDataByReferenceableKey("testkey");

            Assert.HasCount(0, testLookup.GetValues());
        }

        [TestMethod]
        public void TestRemoveDataByReferenceableKeyNotFound()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                testLookup.RemoveDataByReferenceableKey("testkey");
            });
        }

        [TestMethod]
        public void TestTryGetDataById()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            var result = testLookup.TryGetDataById("testid", out var resultData);
            Assert.IsTrue(result);
            Assert.IsNotNull(resultData);
            Assert.AreEqual(mockData, resultData);
        }

        [TestMethod]
        public void TestTryGetDataByIdNotFound()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var result = testLookup.TryGetDataById("testid", out var resultData);
            Assert.IsFalse(result);
            Assert.IsNull(resultData);
        }

        [TestMethod]
        public void TestGetDataById()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            var result = testLookup.GetDataById("testid");
            Assert.IsNotNull(result);
            Assert.AreEqual(mockData, result);
        }

        [TestMethod]
        public void TestGetDataByIdNotFound()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                testLookup.GetDataById("testid");
            });
        }

        [TestMethod]
        public void TestHasDataByReferenceableKey()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            var result = testLookup.HasDataByReferenceableKey("testkey");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestHasDataByReferenceableKeyNotFound()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var result = testLookup.HasDataByReferenceableKey("testkey");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestTryGetDataByReferenceableKey()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            var result = testLookup.TryGetDataByReferenceableKey("testkey", out var resultData);
            Assert.IsTrue(result);
            Assert.IsNotNull(resultData);
            Assert.AreEqual(mockData, resultData);
        }

        [TestMethod]
        public void TestTryGetDataByReferenceableKeyNotFound()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var result = testLookup.TryGetDataByReferenceableKey("testkey", out var resultData);
            Assert.IsFalse(result);
            Assert.IsNull(resultData);
        }

        [TestMethod]
        public void TestGetDataByReferenceableKey()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            var result = testLookup.GetDataByReferenceableKey("testkey");
            Assert.IsNotNull(result);
            Assert.AreEqual(mockData, result);
        }

        [TestMethod]
        public void TestGetDataByReferenceableKeyNotFound()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                testLookup.GetDataByReferenceableKey("testkey");
            });
        }

        [TestMethod]
        public void TestGetIds()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            var result = testLookup.GetIds();
            Assert.HasCount(1, result);
            Assert.AreEqual("testid", result.First());
        }

        [TestMethod]
        public void TestGetIdsEmpty()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var result = testLookup.GetIds();
            Assert.HasCount(0, result);
        }

        [TestMethod]
        public void TestGetValues()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            var result = testLookup.GetValues();
            Assert.HasCount(1, result);
            Assert.AreEqual(mockData, result.First());
        }

        [TestMethod]
        public void TestGetValuesEmpty()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var result = testLookup.GetValues();
            Assert.HasCount(0, result);
        }

        [TestMethod]
        public void TestClear()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            var mockData = Substitute.For<IGlogReferenceable>();
            mockData.GetDataId().Returns("testid");
            mockData.GetReferenceableKey().Returns("testkey");

            testLookup.AddData(mockData);

            Assert.HasCount(1, testLookup.GetValues());

            testLookup.Clear();

            Assert.HasCount(0, testLookup.GetValues());
        }

        [TestMethod]
        public void TestClearFromEmpty()
        {
            var testLookup = new GlogReferenceableLookup<IGlogReferenceable>();

            Assert.HasCount(0, testLookup.GetValues());

            testLookup.Clear();

            Assert.HasCount(0, testLookup.GetValues());
        }
    }
}
