using System;
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

        [IgdbEntityGlogOverrideValue]
        [JsonProperty("abbreviation_glogOverride")]
        public string AbbreviationGlogOverride { get; set; }

        [JsonProperty("alternative_name")]
        public string AlternativeName { get; set; }

        [IgdbEntityId]
        [JsonProperty("id")]
        public int Id { get; set; } = IdNotFound;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        public override string GetReferenceString(IIgdbCache cache)
        {
            if (!string.IsNullOrEmpty(this.AbbreviationGlogOverride))
            {
                return this.AbbreviationGlogOverride;
            }

            if (!string.IsNullOrEmpty(this.Abbreviation))
            {
                return this.Abbreviation;
            }

            if (!string.IsNullOrEmpty(this.AlternativeName))
            {
                return this.AlternativeName;
            }

            return this.Name;
        }
    }
}
