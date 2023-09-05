using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#game
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbGame
    {
        public const int IdNotFound = -1;

        [JsonProperty("category")]
        public IgdbGameCategory Category { get; set; }

        [JsonProperty("checksum")]
        public Guid ChecksumUuid { get; set; }

        [JsonProperty("collection")]
        public int CollectionId { get; set; }

        [JsonProperty("franchise")]
        public int MainFranchiseId { get; set; }

        [JsonProperty("franchises")]
        public List<int> OtherFranchiseIds { get; set; }

        [JsonProperty("game_modes")]
        public List<int> GameModeIds { get; set; }

        [JsonProperty("genres")]
        public List<int> GenreIds { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("involved_companies")]
        public List<int> InvolvedCompanyIds { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("player_perspectives")]
        public List<int> PlayerPerspectiveIds { get; set; }

        [JsonProperty("themes")]
        public List<int> ThemeIds { get; set; }

        [JsonProperty("updated_at")]
        public long UpdatedAtSecondsSinceEpoch { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
