using GlogGenerator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlogGenerator.Test
{
    [TestClass]
    public class VariableSubstitutionTests
    {
        [TestMethod]
        public void TestNothingToDo()
        {
            var vs = new VariableSubstitution();

            var testString = "Nothing to see here";

            var result = vs.TryMakeSubstitutions(testString);

            Assert.AreEqual(testString, result);
        }

        [TestMethod]
        public void TestNoMatchingVariable()
        {
            var vs = new VariableSubstitution();
            vs.SetSubstitution("TestVar", "new value");

            var testString = "$TestVart$ substitution";

            var result = vs.TryMakeSubstitutions(testString);

            Assert.AreEqual(testString, result);
        }

        [TestMethod]
        public void TestMatchingVariable()
        {
            var vs = new VariableSubstitution();
            vs.SetSubstitution("TestVar", "new value");

            var testString = "$TestVar$ substitution";

            var result = vs.TryMakeSubstitutions(testString);

            Assert.AreEqual("new value substitution", result);
        }

        [TestMethod]
        public void TestMultpleMatchingVariables()
        {
            var vs = new VariableSubstitution();
            vs.SetSubstitution("VarOne", "first value");
            vs.SetSubstitution("VarTwo", "second value");

            var testString = "substitute $VarOne$$VarTwo$";

            var result = vs.TryMakeSubstitutions(testString);

            Assert.AreEqual("substitute first valuesecond value", result);
        }
    }
}
