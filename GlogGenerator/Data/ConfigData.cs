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

        public List<string> NowPlaying { get; set; } = new List<string>();

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
