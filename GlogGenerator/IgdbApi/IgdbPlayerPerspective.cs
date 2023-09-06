using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#player-perspective
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbPlayerPerspective
    {
        public const int IdNotFound = -1;

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
