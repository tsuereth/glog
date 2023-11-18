using System;
using System.Collections.Generic;
using System.IO;

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

            var config = new ConfigData()
            {
                InputFilesBasePath = inputFilesBasePath,
                TemplateFilesBasePath = templateFilesBasePath,
            };

            using (var fileReader = File.OpenText(configFilePath))
            {
                var tomlTable = Tommy.TOML.Parse(fileReader);

                if (tomlTable.TryGetNode("now_playing", out var nowPlayingArray))
                {
                    foreach (var nowPlayingEntry in nowPlayingArray)
                    {
                        config.NowPlaying.Add(nowPlayingEntry.ToString());
                    }
                }
            }

            return config;
        }
    }
}
