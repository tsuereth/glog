using GlogGenerator.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test.Data
{
    [TestClass]
    public class UrlizedStringTests
    {
        [TestMethod]
        public void TestUrlizeNoChange()
        {
            var str = "alreadylowercasewithoutpunctuation";

            var urlized = UrlizedString.Urlize(str);

            Assert.AreEqual(str, urlized);
        }

        [TestMethod]
        public void TestUrlizeMixedCaseAndSpace()
        {
            var str = "This should be all lowercase and hyphenated";

            var urlized = UrlizedString.Urlize(str);

            Assert.AreEqual("this-should-be-all-lowercase-and-hyphenated", urlized);
        }

        [TestMethod]
        public void TestUrlizeConsecutivePunctuation()
        {
            var str = "remove:., - those";

            var urlized = UrlizedString.Urlize(str);

            Assert.AreEqual("remove-those", urlized);
        }

        [TestMethod]
        public void TestUrlizeLeadingAndTrailingPunctuation()
        {
            var str = " - trimthat; - ";

            var urlized = UrlizedString.Urlize(str);

            Assert.AreEqual("trimthat", urlized);
        }

        [TestMethod]
        public void TestUrlizedStringEqualsIgnoredDifference()
        {
            var urlized1 = new UrlizedString("Vampire: The Masquerade");
            var urlized2 = new UrlizedString("Vampire the Masquerade");

            Assert.IsTrue(urlized1.Equals(urlized2));
        }

        [TestMethod]
        public void TestUrlizedStringHyphenSpacEqual()
        {
            var urlized1 = new UrlizedString("Take-Two Interactive");
            var urlized2 = new UrlizedString("Take Two Interactive");

            Assert.IsTrue(urlized1.Equals(urlized2));
        }
    }
}
