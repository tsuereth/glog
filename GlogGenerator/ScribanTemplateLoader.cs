using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace GlogGenerator
{
    public class ScribanTemplateLoader : ITemplateLoader
    {
        private const string TemplateFileExtension = ".tmpl";

        private readonly string templateFilesBasePath;

        private Dictionary<string, string> templateFileCache = new Dictionary<string, string>();

        public ScribanTemplateLoader(string templateFilesBasePath)
        {
            this.templateFilesBasePath = templateFilesBasePath;
        }

        private static string GetTemplateFilePath(string basePath, string templateName)
        {
            var templateFileName = $"{templateName}{TemplateFileExtension}";
            return Path.Combine(basePath, templateFileName);
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            // When TemplateA `include`s TemplateB, the given path to TemplateB will be *relative* to TemplateA.
            var basePath = Path.GetDirectoryName(callerSpan.FileName);
            return GetTemplateFilePath(basePath, templateName);
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            if (!this.templateFileCache.TryGetValue(templatePath, out var templateText))
            {
                templateText = File.ReadAllText(templatePath);
                this.templateFileCache[templatePath] = templateText;
            }

            return templateText;
        }

        public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            if (!this.templateFileCache.TryGetValue(templatePath, out var templateText))
            {
                templateText = await File.ReadAllTextAsync(templatePath);
                this.templateFileCache[templatePath] = templateText;
            }

            return templateText;
        }

        private static ScriptObject CreateTemplateGlobalScriptObject(IDictionary<string, object> templateProperties)
        {
            var scriptObject = new ScriptObject();

            scriptObject.Import(typeof(ScribanTemplateFunctions));

            foreach (var property in templateProperties)
            {
                scriptObject.Add(property.Key, property.Value);
            }

            return scriptObject;
        }

        public string ParseAndRender(string templateName, IDictionary<string, object> templateProperties)
        {
            var templateContext = new TemplateContext()
            {
                TemplateLoader = this,

                // The default loop-iteration limit is 1000, which is much lower than (for example) the site's total number of tag items.
                // Overwrite with 0 to indicate NO LIMIT, and cross your fingers ... and watch the build time!
                LoopLimit = 0,

                IndentOnEmptyLines = false,
            };

            var templateGlobals = CreateTemplateGlobalScriptObject(templateProperties);
            templateContext.PushGlobal(templateGlobals);

            var templatePath = GetTemplateFilePath(this.templateFilesBasePath, templateName);

            if (!this.templateFileCache.TryGetValue(templatePath, out var templateText))
            {
                templateText = File.ReadAllText(templatePath);
                this.templateFileCache[templatePath] = templateText;
            }

            var template = Template.Parse(templateText, templatePath);

            if (template.HasErrors)
            {
                throw new InvalidDataException(template.Messages.ToString());
            }

            var rendered = template.Render(templateContext);

            return rendered;
        }
    }
}
