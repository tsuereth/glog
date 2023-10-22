using System.Globalization;
using System.IO;

namespace GlogGenerator.TemplateRenderers
{
    public static class TemplateExtensions
    {
        public static string RenderWithErrorsThrown(this Antlr4.StringTemplate.Template template, CultureInfo culture)
        {
            string rendered;
            using (var stringWriter = new StringWriter())
            {
                var templateWriter = new Antlr4.StringTemplate.AutoIndentWriter(stringWriter);
                var errorListener = new ExceptionThrowingErrorListener();
                template.Write(templateWriter, culture, errorListener);
                rendered = stringWriter.ToString();
            }

            return rendered;
        }
    }
}
