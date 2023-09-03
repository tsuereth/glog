using System.Collections.Generic;
using System.IO;
using GlogGenerator.HugoCompat;

namespace GlogGenerator.Data
{
    public class GameData
    {
        public static readonly string GameContentBaseDir = Path.Combine("content", "game");

        public string PermalinkRelative
        {
            get
            {
                var urlized = TemplateFunctionsStringRenderer.Urlize(this.Title, htmlEncode: true);
                return $"game/{urlized}/";
            }
        }

        public string OutputDirRelative
        {
            get
            {
                var urlizedDir = TemplateFunctionsStringRenderer.Urlize(this.Title, htmlEncode: false, terminologySpecial: true);
                return $"game/{urlizedDir}";
            }
        }

        public string Title { get; set; } = string.Empty;

        public int? IgdbId { get; set; }

        public string IgdbUrl { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        public static GameData FromFilePath(string filePath)
        {
            var fileLines = File.ReadAllLines(filePath);
            var data = FrontMatterToml.FromLines(fileLines);

            var game = new GameData();

            game.Title = data.GetValue<string>("title") ?? string.Empty;
            game.IgdbId = data.GetValue<int?>("igdb_id");
            game.IgdbUrl = data.GetValue<string>("igdb_url") ?? null;
            game.Tags = data.GetValue<List<string>>("tag") ?? new List<string>();

            return game;
        }
    }
}
