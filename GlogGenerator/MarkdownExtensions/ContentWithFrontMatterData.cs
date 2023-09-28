using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Markdig;
using Markdig.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlogGenerator.MarkdownExtensions
{
    public class ContentWithFrontMatterData
    {
        [IgnoreDataMember]
        public string Content { get; private set; } = string.Empty;

        public static T FromFilePath<T>(string filePath)
            where T : ContentWithFrontMatterData, new()
        {
            var text = File.ReadAllText(filePath);

            var mdPipeline = new MarkdownPipelineBuilder()
                .Use<GlogMarkdownWithFrontMatterExtension>()
                .Build();
            var mdDoc = Markdown.Parse(text, mdPipeline);

            var tomlBlock = mdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();
            T contentAndData;
            var contentStartPos = 0;
            if (tomlBlock != null)
            {
                var tomlString = tomlBlock.Content;
                contentAndData = Tomlyn.Toml.ToModel<T>(tomlString);

                contentStartPos = tomlBlock.Span.Start + tomlBlock.Span.Length;
            }
            else
            {
                contentAndData = new T();
            }

            contentAndData.Content = text.Substring(contentStartPos);

            return contentAndData;
        }
    }
}
