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
        public string DataBasePath { get; set; } = string.Empty;
        
        public string BaseURL { get; set; } = "https://localhost:1313/glog/";

        public string Author { get; set; } = string.Empty;

        public string LanguageCode { get; set; } = string.Empty;

        public List<string> NowPlaying { get; set; } = new List<string>();

        public string Title { get; set; } = string.Empty;

        public static ConfigData FromFilePath(string filePath)
        {
            var filePathDir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(filePathDir))
            {
                throw new ArgumentException($"Config filePath {filePath} has empty dirname");
            }
            
            var fileText = File.ReadAllText(filePath);

            var tomlData = Toml.ToModel(fileText);

            var config = new ConfigData();
            config.DataBasePath = filePathDir;

            if (tomlData.TryGetValue("languageCode", out var languageCode))
            {
                config.LanguageCode = languageCode.ToString() ?? string.Empty;
            }

            if (tomlData.TryGetValue("title", out var title))
            {
                config.Title = title.ToString() ?? string.Empty;
            }

            if (tomlData.TryGetValue("author", out var author))
            {
                var authorTable = (TomlTable)author;
                
                if (authorTable.TryGetValue("name", out var authorName))
                {
                    config.Author = authorName.ToString() ?? string.Empty;
                }
            }

            if (tomlData.TryGetValue("params", out var configParams))
            {
                var paramsTable = (TomlTable)configParams;

                if (paramsTable.TryGetValue("now_playing", out var nowPlayingObj))
                {
                    var nowPlayingArray = (TomlArray)nowPlayingObj;

                    config.NowPlaying = nowPlayingArray
                        .Select(o => o?.ToString() ?? string.Empty)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();
                }
            }

            return config;
        }
    }
}
