using GlogGenerator.IgdbApi;
using Newtonsoft.Json;

namespace GlogGenerator.Data
{
    public class IgdbPlatformReference : IgdbEntityReference<IgdbPlatform>, IIgdbEntityReference
    {
        [JsonProperty("nameOverride")]
        public string NameOverride { get; private set; } = null;

        [JsonProperty("igdbPlatformAbbreviation")]
        public string IgdbPlatformAbbreviation { get; private set; } = null;

        [JsonProperty("igdbPlatformAlternativeName")]
        public string IgdbPlatformAlternativeName { get; private set; } = null;

        [JsonProperty("igdbPlatformName")]
        public string IgdbPlatformName { get; private set; } = null;

        public IgdbPlatformReference() : base() { }

        public IgdbPlatformReference(IgdbPlatform fromPlatform) : base(fromPlatform)
        {
            this.NameOverride = fromPlatform.AbbreviationGlogOverride;
            if (this.NameOverride == null)
            {
                // Some platforms are known by an abbreviation, like "GBA" (Game Boy Advance)
                if (!string.IsNullOrEmpty(fromPlatform.Abbreviation))
                {
                    this.IgdbPlatformAbbreviation = fromPlatform.Abbreviation;
                }
                // Some platforms are known by a nickname, like "Vita" (PlayStation Vita)
                else if (!string.IsNullOrEmpty(fromPlatform.AlternativeName))
                {
                    this.IgdbPlatformAlternativeName = fromPlatform.AlternativeName;
                }
                else
                {
                    this.IgdbPlatformName = fromPlatform.Name;
                }
            }
        }

        public override string GetReferenceableKey()
        {
            if (!string.IsNullOrEmpty(this.NameOverride))
            {
                return this.NameOverride;
            }

            if (!string.IsNullOrEmpty(this.IgdbPlatformAbbreviation))
            {
                return this.IgdbPlatformAbbreviation;
            }

            if (!string.IsNullOrEmpty(this.IgdbPlatformAlternativeName))
            {
                return this.IgdbPlatformAlternativeName;
            }

            return IgdbPlatformName;
        }

        public void SetNameOverride(string nameOverride)
        {
            this.NameOverride = nameOverride;
        }

        public virtual void ReapplyCustomPropertiesTo(IgdbPlatformReference target)
        {
            target.NameOverride = this.NameOverride;
        }
    }
}
