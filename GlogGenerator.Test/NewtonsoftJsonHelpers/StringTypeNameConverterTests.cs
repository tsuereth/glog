using System;
using GlogGenerator.NewtonsoftJsonHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace GlogGenerator.Test.NewtonsoftJsonHelpers
{
    [TestClass]
    public class StringTypeNameConverterTests
    {
        class ConverterPropertyTestType { }

        class ConverterObjectTestType
        {
            [JsonConverter(typeof(StringTypeNameConverter))]
            [JsonProperty("default")]
            public Type TestPropertyDefault { get; set; } = null;

            [JsonConverter(typeof(StringTypeNameConverter), "GlogGenerator.Test")]
            [JsonProperty("withAssemblyName")]
            public Type TestPropertyWithAssemblyName { get; set; } = null;

            [JsonConverter(typeof(StringTypeNameConverter), /*null*/"", "GlogGenerator.Test.NewtonsoftJsonHelpers")]
            [JsonProperty("withNamespace")]
            public Type TestPropertyWithNamespace { get; set; } = null;

            [JsonConverter(typeof(StringTypeNameConverter), "GlogGenerator.Test", "GlogGenerator.Test.NewtonsoftJsonHelpers")]
            [JsonProperty("withAssemblyNameAndNamespace")]
            public Type TestPropertyWithAssemblyNameAndNamespace { get; set; } = null;
        }

        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
        };

        [TestMethod]
        public void TestWriteJson()
        {
            var testObject = new ConverterObjectTestType()
            {
                TestPropertyDefault = typeof(ConverterPropertyTestType),
                TestPropertyWithAssemblyName = typeof(ConverterPropertyTestType),
                TestPropertyWithNamespace = typeof(ConverterPropertyTestType),
                TestPropertyWithAssemblyNameAndNamespace = typeof(ConverterPropertyTestType),
            };

            var result = JsonConvert.SerializeObject(testObject, jsonSerializerSettings);

            var expectedJson = @"{" +
                @"""default"":""GlogGenerator.Test:GlogGenerator.Test.NewtonsoftJsonHelpers.StringTypeNameConverterTests+ConverterPropertyTestType""," +
                @"""withAssemblyName"":""GlogGenerator.Test.NewtonsoftJsonHelpers.StringTypeNameConverterTests+ConverterPropertyTestType""," +
                @"""withNamespace"":""GlogGenerator.Test:StringTypeNameConverterTests+ConverterPropertyTestType""," +
                @"""withAssemblyNameAndNamespace"":""StringTypeNameConverterTests+ConverterPropertyTestType""" +
                @"}";
            Assert.AreEqual(expectedJson, result);
        }

        [TestMethod]
        public void TestWriteJsonWithAssemblyNameMismatch()
        {
            var testObject = new ConverterObjectTestType()
            {
                TestPropertyWithAssemblyName = typeof(System.Attribute),
            };

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                var result = JsonConvert.SerializeObject(testObject);
            });
        }

        [TestMethod]
        public void TestWriteJsonWithNamespaceMismatch()
        {
            var testObject = new ConverterObjectTestType()
            {
                TestPropertyWithNamespace = typeof(GlogGenerator.Test.TestLogger),
            };

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                var result = JsonConvert.SerializeObject(testObject);
            });
        }

        [TestMethod]
        public void TestReadJson()
        {
            var testJson = @"{" +
                @"""default"":""GlogGenerator.Test:GlogGenerator.Test.NewtonsoftJsonHelpers.StringTypeNameConverterTests+ConverterPropertyTestType""," +
                @"""withAssemblyName"":""GlogGenerator.Test.NewtonsoftJsonHelpers.StringTypeNameConverterTests+ConverterPropertyTestType""," +
                @"""withNamespace"":""GlogGenerator.Test:StringTypeNameConverterTests+ConverterPropertyTestType""," +
                @"""withAssemblyNameAndNamespace"":""StringTypeNameConverterTests+ConverterPropertyTestType""" +
                @"}";

            var result = JsonConvert.DeserializeObject<ConverterObjectTestType>(testJson);

            Assert.AreEqual(typeof(ConverterPropertyTestType), result.TestPropertyDefault);
            Assert.AreEqual(typeof(ConverterPropertyTestType), result.TestPropertyWithAssemblyName);
            Assert.AreEqual(typeof(ConverterPropertyTestType), result.TestPropertyWithNamespace);
            Assert.AreEqual(typeof(ConverterPropertyTestType), result.TestPropertyWithAssemblyNameAndNamespace);
        }

        [TestMethod]
        public void TestReadJsonWithAssemblyNameMismatch()
        {
            var anotherAssemblyType = typeof(GlogGenerator.NewtonsoftJsonHelpers.StringTypeNameConverter);
            var testJson = @"{" +
                @"""withAssemblyName"":""" + anotherAssemblyType.FullName + @"""" +
                @"}";

            Assert.ThrowsExactly<TypeLoadException>(() =>
            {
                var result = JsonConvert.DeserializeObject<ConverterObjectTestType>(testJson);
            });
        }

        [TestMethod]
        public void TestReadJsonWithNamespaceMismatch()
        {
            var anotherNamespaceType = typeof(GlogGenerator.Test.TestLogger);
            var testJson = @"{" +
                @"""withNamespace"":""GlogGenerator.Test:" + anotherNamespaceType.Name + @"""" +
                @"}";

            Assert.ThrowsExactly<TypeLoadException>(() =>
            {
                var result = JsonConvert.DeserializeObject<ConverterObjectTestType>(testJson);
            });
        }

        [TestMethod]
        public void TestReadJsonUnknownType()
        {
            var testJson = @"{" +
                @"""withAssemblyNameAndNamespace"":""MadeUpTypeName""" +
                @"}";

            Assert.ThrowsExactly<TypeLoadException>(() =>
            {
                var result = JsonConvert.DeserializeObject<ConverterObjectTestType>(testJson);
            });
        }
    }
}
