﻿using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#keyword
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbKeyword : IgdbEntity
    {
        [IgdbEntityId]
        [JsonProperty("id")]
        public int Id { get; set; }

        [IgdbEntityReferenceableValue]
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
    }
}
