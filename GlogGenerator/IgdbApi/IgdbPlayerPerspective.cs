using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#player-perspective
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbPlayerPerspective : IgdbEntity
    {
        [IgdbEntityId]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        public override string GetReferenceString(IIgdbCache cache)
        {
            return this.Name;
        }
    }
}
