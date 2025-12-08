using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#game-type
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbGameType : IgdbEntity
    {
        [IgdbEntityId]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        public override string GetReferenceString(IIgdbCache cache)
        {
            return this.Type;
        }

        // The "Bundle" type is specially handled, so recognize that type specifically.
        public bool IsBundle()
        {
            return this.Type.Equals("Bundle", System.StringComparison.Ordinal);
        }
    }
}
