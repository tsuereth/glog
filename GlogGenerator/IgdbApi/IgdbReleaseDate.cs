using Newtonsoft.Json;
using System;

namespace GlogGenerator.IgdbApi
{
    // https://api-docs.igdb.com/#release-date
    // This class is NOT a complete representation, it only includes properties as-needed.
    public class IgdbReleaseDate : IgdbEntity
    {
        [JsonProperty("date")]
        public long DateTimestamp { get; set; } = 0;

        [JsonIgnore]
        public DateTimeOffset Date
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(this.DateTimestamp);
            }
        }

        [IgdbEntityId]
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
