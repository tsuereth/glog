using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#collection
    // This class is NOT a complete representation, it only includes properties as-needed.
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class IgdbCollection : IgdbEntity
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
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
}
