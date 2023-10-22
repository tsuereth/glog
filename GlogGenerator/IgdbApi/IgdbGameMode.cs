using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#game-mode
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbGameMode : IgdbEntity
    {
        [IgdbEntityId]
        [JsonProperty("id")]
        public int Id { get; set; }

        [IgdbEntityReferenceableKey]
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
