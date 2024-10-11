using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#game
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbGame : IgdbEntity
    {
        [JsonProperty("bundles")]
        public List<int> BundleGameIds { get; set; } = new List<int>();

        [JsonProperty("category")]
        public IgdbGameCategory Category { get; set; } = IgdbGameCategory.None;

        [JsonProperty("collection")]
        public int MainCollectionId { get; set; } = IgdbCollection.IdNotFound;

        [JsonProperty("collections")]
        public List<int> CollectionIds { get; set; } = new List<int>();

        [JsonProperty("dlcs")]
        public List<int> DlcGameIds { get; set; } = new List<int>();

        [JsonProperty("expanded_games")]
        public List<int> ExpandedGameIds { get; set; } = new List<int>();

        [JsonProperty("expansions")]
        public List<int> ExpansionGameIds { get; set; } = new List<int>();

        [JsonProperty("first_release_date")]
        public long FirstReleaseDateTimestamp { get; set; } = 0;

        [JsonIgnore]
        public DateTimeOffset FirstReleaseDate
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(this.FirstReleaseDateTimestamp);
            }
        }

        [JsonProperty("forks")]
        public List<int> ForkGameIds { get; set; } = new List<int>();

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

        [JsonProperty("keywords")]
        public List<int> KeywordIds { get; set; } = new List<int>();

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [IgdbEntityGlogOverrideValue]
        [JsonProperty("name_glogAppendReleaseYear")]
        public bool? NameGlogAppendReleaseYear { get; set; } = null;

        [IgdbEntityGlogOverrideValue]
        [JsonProperty("name_glogOverride")]
        public string NameGlogOverride { get; set; } = null;

        [IgdbEntityReferenceableValue]
        [JsonIgnore]
        public string NameForGlog
        {
            get
            {
                if (!string.IsNullOrEmpty(this.NameGlogOverride))
                {
                    return this.NameGlogOverride;
                }

                if (this.NameGlogAppendReleaseYear == true)
                {
                    return $"{this.Name} ({this.FirstReleaseDate.Year})";
                }

                return this.Name;
            }
        }

        [JsonProperty("parent_game")]
        public int ParentGameId { get; set; } = IdNotFound;

        [JsonProperty("player_perspectives")]
        public List<int> PlayerPerspectiveIds { get; set; } = new List<int>();

        [JsonProperty("ports")]
        public List<int> PortGameIds { get; set; } = new List<int>();

        [JsonProperty("remakes")]
        public List<int> RemakeGameIds { get; set; } = new List<int>();

        [JsonProperty("remasters")]
        public List<int> RemasterGameIds { get; set; } = new List<int>();

        [JsonProperty("standalone_expansions")]
        public List<int> StandaloneExpansionGameIds { get; set; } = new List<int>();

        [JsonProperty("themes")]
        public List<int> ThemeIds { get; set; } = new List<int>();

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("version_parent")]
        public int VersionParentGameId { get; set; } = IdNotFound;
    }
}
