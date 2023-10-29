using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlogGenerator.MarkdownExtensions
{
    public class ContentWithFrontMatterData
    {
        [IgnoreDataMember]
        public MarkdownDocument Content { get; private set; }

        public static T FromFilePath<T>(MarkdownPipeline mdPipeline, string filePath)
            where T : ContentWithFrontMatterData, new()
        {
            var text = File.ReadAllText(filePath);

            var mdDoc = Markdown.Parse(text, mdPipeline);

            var tomlBlock = mdDoc.Descendants<TomlFrontMatterBlock>().FirstOrDefault();
            T contentAndData;
            if (tomlBlock != null)
            {
                var tomlString = tomlBlock.Content;
                contentAndData = Tomlyn.Toml.ToModel<T>(tomlString);

                mdDoc.Remove(tomlBlock);
            }
            else
            {
                contentAndData = new T();
            }

            contentAndData.Content = mdDoc;

            return contentAndData;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            var tomlOutOptions = new Tomlyn.TomlModelOptions()
            {
                IgnoreMissingProperties = true,
            };
            var frontMatterText = Tomlyn.Toml.FromModel(this, tomlOutOptions);

            if (!string.IsNullOrEmpty(frontMatterText))
            {
                stringBuilder.AppendLine("+++");
                stringBuilder.Append(frontMatterText);
                stringBuilder.AppendLine("+++");
            }

            using (var mdTextWriter = new StringWriter())
            {
                var mdRenderer = new NormalizeRenderer(mdTextWriter);
                mdRenderer.Render(this.Content);

                stringBuilder.Append(mdTextWriter.ToString());
            }

            // Always end the file with a line break.
            stringBuilder.AppendLine();

            return stringBuilder.ToString();
        }
    }
}
