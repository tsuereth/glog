using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;

namespace GlogGenerator.Data
{
    public class ConfigData
    {
        public string BaseURL { get; set; } = "https://localhost:1313/glog/";

        public string InputFilesBasePath { get; set; } = Directory.GetCurrentDirectory();

        public List<string> NowPlaying { get; set; } = new List<string>();

        public string TemplateFilesBasePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "templates");

        public static ConfigData FromFilePaths(string configFilePath, string inputFilesBasePath, string templateFilesBasePath)
        {
            var filePathDir = Path.GetDirectoryName(configFilePath);
            if (string.IsNullOrEmpty(filePathDir))
            {
                throw new ArgumentException($"Config filePath {configFilePath} has empty dirname");
            }
            
            var fileText = File.ReadAllText(configFilePath);

            var tomlData = Toml.ToModel(fileText);

            var config = new ConfigData()
            {
                InputFilesBasePath = inputFilesBasePath,
                TemplateFilesBasePath = templateFilesBasePath,
            };

            if (tomlData.TryGetValue("now_playing", out var nowPlayingObj))
            {
                var nowPlayingArray = (TomlArray)nowPlayingObj;

                config.NowPlaying = nowPlayingArray
                    .Select(o => o?.ToString() ?? string.Empty)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
            }

            return config;
        }
    }
}
