﻿using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using GlogGenerator.Data;
using GlogGenerator.RenderState;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlogGenerator.MarkdownExtensions
{
    public class FencedDataBlockHtmlRenderer : HtmlObjectRenderer<FencedDataBlock>
    {
        private readonly ISiteDataIndex siteDataIndex;
        private readonly HtmlRendererContext htmlRendererContext;

        public FencedDataBlockHtmlRenderer(
            ISiteDataIndex siteDataIndex,
            HtmlRendererContext htmlRendererContext)
        {
            this.siteDataIndex = siteDataIndex;
            this.htmlRendererContext = htmlRendererContext;
        }

        protected override void Write(HtmlRenderer renderer, FencedDataBlock obj)
        {
            if (obj.Info.Equals("chart", StringComparison.Ordinal))
            {
                var namedArgs = obj.Data.ToDictionary(kv => kv.Key, kv => kv.Value.ToString().Trim('"'));

                var datafileArg = namedArgs["datafile"].ToString();
                // If this filepath "looks" absolute, it ... isn't!
                // It's relative to the site's input base path, so, un-absolute it.
                if (datafileArg[0] == '/')
                {
                    datafileArg = datafileArg.Remove(0, 1);
                }

                var chartDatafileContent = this.siteDataIndex.GetRawDataFile(datafileArg);
                if (string.IsNullOrEmpty(chartDatafileContent))
                {
                    throw new InvalidDataException($"Failed to read datafile path {datafileArg}");
                }

                // We need to escape the JSON data, to make it safe for JavaScript to load as a string.
                var chartDatafileJson = JObject.Parse(chartDatafileContent);
                var chartDatafileJsonString = JsonConvert.SerializeObject(chartDatafileJson, Formatting.None);
                var chartDataString = HttpUtility.JavaScriptStringEncode(chartDatafileJsonString);

                string chartType;
                if (namedArgs.TryGetValue("type", out var chartTypeArg))
                {
                    chartType = chartTypeArg.ToString();
                }
                else
                {
                    chartType = "BarChart";
                }

                var chartOptionsPairs = namedArgs.Where(kv => !kv.Key.Equals("datafile", StringComparison.OrdinalIgnoreCase) && !kv.Key.Equals("type", StringComparison.OrdinalIgnoreCase));
                var chartOptionsTextBuilder = new StringBuilder();
                foreach (var chartOptionsPair in chartOptionsPairs.ToImmutableSortedDictionary())
                {
                    // FIXME: instead of using string-encoded JSON in the markup, ... use JSON!
                    var valueIsJson = chartOptionsPair.Value.ToString().StartsWith('{') || chartOptionsPair.Value.ToString().StartsWith('[');
                    var optionKey = chartOptionsPair.Key;
                    string optionValue;
                    if (valueIsJson)
                    {
                        // The raw value has escaped quotation marks, we need to un-escape them.
                        optionValue = chartOptionsPair.Value.ToString().Replace("\\\"", "\"");
                    }
                    else
                    {
                        // The raw value needs to be quoted.
                        optionValue = $"'{chartOptionsPair.Value}'";
                    }

                    chartOptionsTextBuilder.AppendLine();
                    chartOptionsTextBuilder.AppendLine();
                    chartOptionsTextBuilder.AppendLine(CultureInfo.InvariantCulture, $"\toptions['{optionKey}'] = {optionValue};");
                    chartOptionsTextBuilder.AppendLine();
                }

                var pageHash = this.htmlRendererContext.GetPageHashCode();

                var chartHashInString = JsonConvert.SerializeObject(namedArgs);
                var chartHashInBytes = Encoding.UTF8.GetBytes(chartHashInString);
                var chartHashOutBytes = SHA256.HashData(chartHashInBytes);
                var chartHash = Convert.ToHexString(chartHashOutBytes);

                var chartMarkup = $@"<div class=""center""><chart id=""chart_{pageHash}_{chartHash}"" callback=""drawChart_{pageHash}_{chartHash}""></chart></div>
<script type=""text/javascript"">
function drawChart_{pageHash}_{chartHash}() {{
	var data = new google.visualization.DataTable(""{chartDataString}"");
	var options = {{}};
{chartOptionsTextBuilder.ToString()}
	var element = document.getElementById(""chart_{pageHash}_{chartHash}"");
	var chart = new google.visualization.{chartType}(element);
	chart.draw(data, options);
}}
google.charts.setOnLoadCallback(drawChart_{pageHash}_{chartHash});
</script>
<noscript><i>A Google Chart would go here, but JavaScript is disabled.</i></noscript>
";

                renderer.Write(chartMarkup);
            }
        }
    }
}
