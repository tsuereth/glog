using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Markdig;
using Markdig.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlogGenerator.MarkdownExtensions
{
    public class ContentWithFrontMatterData
    {
        private Dictionary<string, JToken> frontMatterValues = new Dictionary<string, JToken>();

        public string Content { get; private set; } = string.Empty;

        public T GetValue<T>(string key)
        {
            if (!this.frontMatterValues.ContainsKey(key) || this.frontMatterValues[key].Type == JTokenType.Null)
            {
                return default;
            }

            return this.frontMatterValues[key].ToObject<T>() ?? throw new InvalidDataException($"Failed to convert \"{key}\" value {this.frontMatterValues[key].ToString()}");
        }

        public static ContentWithFrontMatterData FromFilePath(string filePath)
        {
            var text = File.ReadAllText(filePath);

            var mdPipeline = new MarkdownPipelineBuilder()
                .Use<GlogMarkdownWithFrontMatterExtension>()
                .Build();
            var mdDoc = Markdown.Parse(text, mdPipeline);

            var data = new ContentWithFrontMatterData();

            var tomlBlock = mdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();
            var contentStartPos = 0;
            if (tomlBlock != null)
            {
                var tomlString = text.Substring(tomlBlock.Span.Start, tomlBlock.Span.Length);

                // FIXME: use a "real" toml parser!

                var lines = tomlString.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    if (line.Equals("+++", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var keyAndValue = line.Split(new char[] { '=', ':' }, 2, StringSplitOptions.TrimEntries);

                    if (string.IsNullOrEmpty(keyAndValue[1]))
                    {
                        data.frontMatterValues[keyAndValue[0]] = JValue.CreateNull();
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

                        data.frontMatterValues[keyAndValue[0]] = valueJson;
                    }
                }

                contentStartPos = tomlBlock.Span.Start + tomlBlock.Span.Length;
            }

            data.Content = text.Substring(contentStartPos);

            return data;
        }
    }
}
