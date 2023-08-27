using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace GlogGenerator.HugoCompat
{
    public class FrontMatterToml
    {
        private Dictionary<string, JToken> frontMatterValues = new Dictionary<string, JToken>();

        private string text = string.Empty;

        public T GetValue<T>(string key)
        {
            if (!this.frontMatterValues.ContainsKey(key) || this.frontMatterValues[key].Type == JTokenType.Null)
            {
                return default;
            }

            return this.frontMatterValues[key].ToObject<T>() ?? throw new InvalidDataException($"Failed to convert \"{key}\" value {this.frontMatterValues[key].ToString()}");
        }

        public string GetText()
        {
            return this.text;
        }

        public static FrontMatterToml FromFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);

            return FromLines(lines);
        }

        public static FrontMatterToml FromLines(string[] lines)
        {
            var fmToml = new FrontMatterToml();

            bool? inFrontMatter = null;
            var textBuilder = new StringBuilder();
            var textWriter = new StringWriter(textBuilder)
            {
                NewLine = "\n",
            };

            foreach (var line in lines)
            {
                if (line.Equals("+++", StringComparison.Ordinal) || line.Equals("---", StringComparison.Ordinal))
                {
                    if (!inFrontMatter.HasValue)
                    {
                        inFrontMatter = true;
                    }
                    else if (inFrontMatter == true)
                    {
                        inFrontMatter = false;
                    }
                    else
                    {
                        throw new InvalidDataException("Unexpected front-matter delimiter after front-matter was already found");
                    }
                }
                else if (inFrontMatter == true)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var keyAndValue = line.Split(new char[] { '=', ':' }, 2, StringSplitOptions.TrimEntries);

                        if (string.IsNullOrEmpty(keyAndValue[1]))
                        {
                            fmToml.frontMatterValues[keyAndValue[0]] = JValue.CreateNull();
                        }
                        else
                        {
                            // All values are quoted strings or JSON-like arrays or objects.
                            // And since a quoted string is JSON, too...!
                            // Some strings are date/time stamps, and we DON'T want JsonConvert
                            // to attempt to understand those, because it loses timezone info.
                            var jsonSerializerSettings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None };
                            var valueJson = JsonConvert.DeserializeObject<JToken>(keyAndValue[1], jsonSerializerSettings)
                                ?? throw new InvalidDataException($"Failed to parse \"{keyAndValue[0]}\" value {keyAndValue[1]}");

                            fmToml.frontMatterValues[keyAndValue[0]] = valueJson;
                        }
                    }
                }
                else
                {
                    textWriter.WriteLine(line);
                }
            }

            fmToml.text = textBuilder.ToString();

            return fmToml;
        }
    }
}
