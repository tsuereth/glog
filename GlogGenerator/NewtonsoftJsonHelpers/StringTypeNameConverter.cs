using System;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace GlogGenerator.NewtonsoftJsonHelpers
{
    public class StringTypeNameConverter : JsonConverter
    {
        public string AssemblyName { get; set; } = null;

        public string TypeNamespace { get; set; } = null;

        public StringTypeNameConverter() { }

        public StringTypeNameConverter(string assemblyName)
        {
            this.AssemblyName = assemblyName;
        }

        public StringTypeNameConverter(string assemblyName, string typeNamespace)
        {
            this.AssemblyName = assemblyName;
            this.TypeNamespace = typeNamespace;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Type t = (Type)value;

            // TODO: Is there a better way to write characters to the json writer without encapsulating "a value" ?
            var valueBuilder = new StringBuilder();

            var tAssemblyName = t.Assembly.GetName().Name;
            if (!string.IsNullOrEmpty(this.AssemblyName))
            {
                if (tAssemblyName != this.AssemblyName)
                {
                    throw new ArgumentException($"{nameof(StringTypeNameConverter)} AssemblyName '{this.AssemblyName}' does not match object's assembly name '{tAssemblyName}'");
                }
            }
            else
            {
                valueBuilder.Append(tAssemblyName);
                valueBuilder.Append(':');
            }

            var tFullName = t.FullName;
            if (!string.IsNullOrEmpty(this.TypeNamespace))
            {
                var tNamespacePrefix = this.TypeNamespace + '.';
                if (!tFullName.StartsWith(tNamespacePrefix, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"{nameof(StringTypeNameConverter)} TypeNamespace '{this.TypeNamespace}' does not match object's type '{tFullName}'");
                }

                var tName = tFullName.Substring(tNamespacePrefix.Length);
                valueBuilder.Append(tName);
            }
            else
            {
                valueBuilder.Append(tFullName);
            }

            writer.WriteValue(valueBuilder.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new ArgumentException($"{nameof(StringTypeNameConverter)} cannot read a non-string value");
            }

            var valueString = reader.Value?.ToString();
            if (string.IsNullOrEmpty(valueString))
            {
                return null;
            }

            string tAssemblyName;
            if (!string.IsNullOrEmpty(this.AssemblyName))
            {
                tAssemblyName = this.AssemblyName;
            }
            else
            {
                var valueParts = valueString.Split(':', 2);
                if (valueParts.Length < 2)
                {
                    throw new ArgumentException($"{nameof(StringTypeNameConverter)} with empty AssemblyName cannot read value with no assembly prefix");
                }

                tAssemblyName = valueParts[0];
                valueString = valueParts[1];
            }

            string tFullName;
            if (!string.IsNullOrEmpty(this.TypeNamespace))
            {
                tFullName = this.TypeNamespace + '.' + valueString;
            }
            else
            {
                tFullName = valueString;
            }

            var assembly = Assembly.Load(tAssemblyName);
            if (assembly == null)
            {
                throw new ArgumentException($"{nameof(StringTypeNameConverter)} failed to resolve assembly named '{tAssemblyName}'");
            }

            var t = assembly.GetType(tFullName, throwOnError: true);

            return t;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetType() == typeof(Type);
        }
    }
}
