using GlogGenerator.IgdbApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GlogGenerator.Data
{
    public class IgdbPlatformReference : IgdbEntityReference<IgdbPlatform>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ReferenceNameSourceType
        {
            None,
            IgdbPlatformAbbreviation,
            IgdbPlatformAlternativeName,
            IgdbPlatformName,
        };

        [JsonProperty("referenceName")]
        public string ReferenceName { get; private set; }

        [JsonProperty("referenceNameSource")]
        public ReferenceNameSourceType ReferenceNameSource { get; private set; } = ReferenceNameSourceType.None;

        public IgdbPlatformReference(IgdbPlatform fromPlatform) : base(fromPlatform)
        {
            // Some platforms are known by an abbreviation, like "GBA" (Game Boy Advance)
            if (!string.IsNullOrEmpty(fromPlatform.Abbreviation))
            {
                this.ReferenceName = fromPlatform.Abbreviation;
                this.ReferenceNameSource = ReferenceNameSourceType.IgdbPlatformAbbreviation;
            }
            // Some platforms are known by a nickname, like "Vita" (PlayStation Vita)
            else if (!string.IsNullOrEmpty(fromPlatform.AlternativeName))
            {
                this.ReferenceName = fromPlatform.AlternativeName;
                this.ReferenceNameSource = ReferenceNameSourceType.IgdbPlatformAlternativeName;
            }
            else
            {
                this.ReferenceName = fromPlatform.Name;
                this.ReferenceNameSource = ReferenceNameSourceType.IgdbPlatformName;
            }
        }

        public override string GetReferenceableKey()
        {
            return ReferenceName;
        }
    }
}
