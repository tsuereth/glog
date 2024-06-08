using System;
using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#involved-company
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbInvolvedCompany : IgdbEntity
    {
        [IgdbEntityId]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("company", Required = Required.Always)]
        public int CompanyId { get; set; }
    }
}
