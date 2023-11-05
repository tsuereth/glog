﻿using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#company
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbCompany : IgdbEntity
    {
        [IgdbEntityId]
        [JsonProperty("id")]
        public int Id { get; set; }

        [IgdbEntityReferenceableValue]
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
