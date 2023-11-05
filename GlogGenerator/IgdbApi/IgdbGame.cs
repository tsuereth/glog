using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#game
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbGame : IgdbEntity
    {
        [JsonProperty("category")]
        public IgdbGameCategory Category { get; set; } = IgdbGameCategory.None;

        [JsonProperty("collection")]
        public int MainCollectionId { get; set; } = IgdbCollection.IdNotFound;

        [JsonProperty("collections")]
        public List<int> CollectionIds { get; set; } = new List<int>();

        [JsonProperty("franchise")]
        public int MainFranchiseId { get; set; } = IgdbFranchise.IdNotFound;

        [JsonProperty("franchises")]
        public List<int> FranchiseIds { get; set; } = new List<int>();

        [JsonProperty("game_modes")]
        public List<int> GameModeIds { get; set; } = new List<int>();

        [JsonProperty("genres")]
        public List<int> GenreIds { get; set; } = new List<int>();

        [IgdbEntityId]
        [JsonProperty("id")]
        public int Id { get; set; } = IdNotFound;

        [JsonProperty("involved_companies")]
        public List<int> InvolvedCompanyIds { get; set; } = new List<int>();

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_glogOverride")]
        public string NameGlogOverride { get; set; }

        [IgdbEntityReferenceableValue]
        [JsonIgnore]
        public string NameForGlog
        {
            get
            {
                return this.NameGlogOverride ?? this.Name;
            }
        }

        [JsonProperty("player_perspectives")]
        public List<int> PlayerPerspectiveIds { get; set; } = new List<int>();

        [JsonProperty("themes")]
        public List<int> ThemeIds { get; set; } = new List<int>();

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
