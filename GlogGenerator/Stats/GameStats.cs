using System;

namespace GlogGenerator.Stats
{
    public class GameStats
    {
        public string Title { get; set; } = string.Empty;

        public string Platform { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public DateTimeOffset? FirstPosted { get; set; } = null;

        public DateTimeOffset? LastPosted { get; set; } = null;

        public string Rating { get; set; } = string.Empty;

        public int NumPosts { get; set; } = 0;
    }
}
