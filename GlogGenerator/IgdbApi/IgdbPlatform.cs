﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#platform
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbPlatform : IgdbEntity
    {
        [JsonProperty("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonProperty("abbreviation_glogOverride")]
        public string AbbreviationGlogOverride { get; set; }

        [JsonIgnore]
        public string AbbreviationForGlog
        {
            get
            {
                return this.AbbreviationGlogOverride ?? this.Abbreviation;
            }
        }

        [IgdbEntityId]
        [JsonProperty("id")]
        public int Id { get; set; } = IdNotFound;

        [IgdbEntityReferenceableKey]
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
