using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#game
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbGame : IgdbEntity
    {
        [JsonProperty("bundles")]
        public List<int> BundleGameIds { get; set; } = new List<int>();

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

        [JsonProperty("forks")]
        public List<int> ForkGameIds { get; set; } = new List<int>();

        [JsonProperty("franchise")]
        public int MainFranchiseId { get; set; } = IgdbFranchise.IdNotFound;

        [JsonProperty("franchises")]
        public List<int> FranchiseIds { get; set; } = new List<int>();

        [JsonProperty("game_modes")]
        public List<int> GameModeIds { get; set; } = new List<int>();

        [JsonProperty("game_type")]
        public int GameTypeId { get; set; } = IgdbGameType.IdNotFound;

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

        [JsonProperty("parent_game")]
        public int ParentGameId { get; set; } = IdNotFound;

        [JsonProperty("platforms")]
        public List<int> PlatformIds { get; set; } = new List<int>();

        [JsonProperty("player_perspectives")]
        public List<int> PlayerPerspectiveIds { get; set; } = new List<int>();

        [JsonProperty("ports")]
        public List<int> PortGameIds { get; set; } = new List<int>();

        [JsonProperty("release_dates")]
        public List<int> ReleaseDateIds { get; set; } = new List<int>();

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

        public DateTimeOffset? GetFirstReleaseDate(IIgdbCache cache)
        {
            if (this.FirstReleaseDateTimestamp != 0)
            {
                return DateTimeOffset.FromUnixTimeSeconds(this.FirstReleaseDateTimestamp);
            }

            var releaseDates = this.ReleaseDateIds.Select(id => cache.GetReleaseDate(id)).Where(d => d != null && d.DateTimestamp != 0);
            if (releaseDates.Any())
            {
                return releaseDates.OrderBy(d => d.DateTimestamp).First().Date;
            }

            return null;
        }

        public IEnumerable<int> GetParentGameIds()
        {
            // DO NOT include ParentGameId in this!
            // The meaning of ParentGameId varies by this game's category --
            // It may indicate a DLC or expansion's parent game, or a collected game's bundle;
            // Or, it may indicate a remaster's original release, or a standalone expansion's preceding game.

            return this.BundleGameIds
                .Where(i => i != IgdbEntity.IdNotFound);
        }

        public IEnumerable<int> GetOtherReleaseGameIds()
        {
            var otherReleaseGameIds = new List<int>()
            {
                this.VersionParentGameId,
            };

            return otherReleaseGameIds
                .Union(this.ExpandedGameIds)
                .Union(this.PortGameIds)
                .Union(this.RemakeGameIds)
                .Union(this.RemasterGameIds)
                .Where(i => i != IgdbEntity.IdNotFound);
        }

        public IEnumerable<int> GetChildGameIds()
        {
            return this.DlcGameIds
                .Union(this.ExpansionGameIds)
                .Where(i => i != IgdbEntity.IdNotFound);
        }

        public IEnumerable<int> GetRelatedGameIds()
        {
            // Forks don't seem to be(?) "other releases" of this game, but totally different, spun-off games.
            // And Standalone Expansions are, well, standalone -- really separate games from the original.
            return this.ForkGameIds
                .Union(this.StandaloneExpansionGameIds)
                .Where(i => i != IgdbEntity.IdNotFound);
        }
    }
}
