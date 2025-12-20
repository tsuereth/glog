using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GlogGenerator.IgdbApi;
using Newtonsoft.Json;

namespace GlogGenerator.Data
{
    public class IgdbGameReference : IgdbEntityReference<IgdbGame>, IIgdbEntityReference
    {
        [JsonProperty("nameOverride")]
        public string NameOverride { get; private set; } = null;

        [JsonProperty("igdbGameName")]
        public string Name { get; private set; } = null;

        // If multiple games have the same name, they're often disambiguated by release year.
        // (And if a game is re-released i.e. on more platforms, the original/first release year is used.)
        //
        // This may occur when the same name is reused by a wholly unrelated game, for example:
        //   "The Witness" was a text adventure game, released in 1983 for personal computers;
        //   "The Witness (2016)" was an open-world puzzle game, released in 2016 for multiple platforms.
        //
        // This may also occur when a game is remade or rebooted and re-uses its earlier name, for example:
        //   "Prince of Persia" was a side-scrolling action adventure game, released in 1989 on Apple II;
        //   "Prince of Persia (2008)" was a 3D third-person franchise reboot, released in 2008 on multiple platforms.
        [JsonProperty("nameAppendReleaseYear")]
        public bool? NameAppendReleaseYear { get; private set; } = null;
        [JsonProperty("igdbGameFirstReleaseYear")]
        public int? FirstReleaseYear { get; private set; } = null;

        // Sometimes, multiple games with the same name are disambiguated by platform.
        // (And if a game is re-released on additional platforms, those new platforms will be included too.)
        //
        // This may occur when the same name is used by meaningfully distinct games released at the same time, for example:
        //   "Ghostbusters: The Video Game (PC, PS3, X360)" was a realism-styled game, released in 2009 for high-power platforms;
        //   "Ghostbusters: The Video Game (PS2, PSP, Wii)" was a cartoon-styled game, released in 2009 for low-power platforms.
        //
        // This may also occur when a game's name is confusingly ambiguous, but colloquially associated with its platform, for example:
        //   "Final Fantasy II" was the second game in Square's RPG series, released in 1988 for Famicom in Japan;
        //   "Final Fantasy II (SNES, Wii)" was the North American rebrand of their FOURTH game, released in 1991 for Super NES.
        [JsonProperty("nameAppendReleasePlatforms")]
        public bool? NameAppendReleasePlatforms { get; private set; } = null;
        [JsonProperty("igdbGameReleasePlatformNames")]
        public List<string> ReleasePlatformNames { get; private set; } = null;

        // There are occasions when target platforms, and the release year, still aren't enough to disambiguate some games.
        // In these cases we just want "some" characteristic which uniquely distinguishes the games,
        // and a Release Number - relative order in which the games were released - can provide that uniqueness.
        //
        // This may occur when the same name is used by meaningfully distinct games with UNCLEAR platform distinctions, for example:
        //   "Sonic the Hedgehog (Handheld LCD Game)" was one version of Sonic, released in 1991 as a Tiger Electronics handheld;
        //   "Sonic the Hedgehog (Handheld LCD Game) (2)" was another version of Sonic, released in 1991 as a Tiger Electronics wristwatch.
        //
        // This may also occur when the source database has a duplicate entry for the same game, in error.
        [JsonProperty("nameAppendReleaseNumber")]
        public bool? NameAppendReleaseNumber { get; private set; } = null;
        [JsonProperty("igdbGameReleaseNumber")]
        public int? ReleaseNumber { get; private set; } = null;

        public IgdbGameReference() : base() { }

        public IgdbGameReference(IgdbGame fromGame, IIgdbCache cache) : base(fromGame)
        {
            // FIXME: There should be no such thing as "empty IGDB game name," but...
            // non-IGDB games currently injected into the IGDB cache have an empty Name.
            // Those non-IGDB games should, ultimately, not be `IgdbGame`s at all. (right?)
            if (!string.IsNullOrEmpty(fromGame.Name))
            {
                this.Name = fromGame.Name;
            }

            this.NameOverride = fromGame.NameGlogOverride;
            if (this.NameOverride == null)
            {
                this.NameAppendReleaseYear = fromGame.NameGlogAppendReleaseYear;
                if (this.NameAppendReleaseYear == true)
                {
                    var firstReleaseDate = fromGame.GetFirstReleaseDate(cache);
                    this.FirstReleaseYear = firstReleaseDate.HasValue ? firstReleaseDate.Value.Year : null;
                }

                this.NameAppendReleasePlatforms = fromGame.NameGlogAppendPlatforms;
                if (this.NameAppendReleasePlatforms == true)
                {
                    this.ReleasePlatformNames = fromGame.PlatformIds
                        .Select(platformId => cache.GetPlatform(platformId))
                        .Select(platform => platform.GetReferenceString(cache))
                        .Order(StringComparer.OrdinalIgnoreCase).ToList();
                }

                this.NameAppendReleaseNumber = fromGame.NameGlogAppendReleaseNumber.HasValue ? true : null;
                if (this.NameAppendReleaseNumber == true)
                {
                    this.ReleaseNumber = fromGame.NameGlogAppendReleaseNumber.Value;
                }
            }
        }

        public override string GetReferenceableKey()
        {
            if (!string.IsNullOrEmpty(this.NameOverride))
            {
                return this.NameOverride;
            }

            var nameBuilder = new StringBuilder();
            nameBuilder.Append(this.Name);

            if (this.NameAppendReleaseYear == true)
            {
                if (!this.FirstReleaseYear.HasValue)
                {
                    throw new InvalidDataException($"Game reference with ID {this.IgdbEntityId} named \"{this.Name}\" is set to append a release year to its name, but has no valid release year.");
                }

                nameBuilder.Append(" (");
                nameBuilder.Append(this.FirstReleaseYear.Value);
                nameBuilder.Append(")");
            }

            if (this.NameAppendReleasePlatforms == true)
            {
                if (this.ReleasePlatformNames == null || this.ReleasePlatformNames.Count == 0)
                {
                    throw new InvalidDataException($"Game ID {this.IgdbEntityId} named \"{this.Name}\" is set to append platforms to its name, but has no valid platforms.");
                }

                nameBuilder.Append(" (");
                nameBuilder.Append(string.Join(", ", this.ReleasePlatformNames));
                nameBuilder.Append(")");
            }

            if (this.NameAppendReleaseNumber == true)
            {
                if (!this.ReleaseNumber.HasValue)
                {
                    throw new InvalidDataException($"Game ID {this.IgdbEntityId} named \"{this.Name}\" is set to append a release number to its name, but has no valid release number.");
                }

                nameBuilder.Append(" (");
                nameBuilder.Append(this.ReleaseNumber.Value.ToString(CultureInfo.InvariantCulture));
                nameBuilder.Append(")");
            }

            return nameBuilder.ToString();
        }

        public void SetNameOverride(string nameOverride)
        {
            this.NameOverride = nameOverride;
        }

        public void SetNameAppendReleaseYear(int firstReleaseYear)
        {
            this.NameAppendReleaseYear = true;
            this.FirstReleaseYear = firstReleaseYear;
        }

        public void SetNameAppendReleasePlatforms(List<string> releasePlatformNames)
        {
            this.NameAppendReleasePlatforms = true;
            this.ReleasePlatformNames = releasePlatformNames;
        }

        public void SetNameAppendReleaseNumber(int releaseNumber)
        {
            this.NameAppendReleaseNumber = true;
            this.ReleaseNumber = releaseNumber;
        }

        public virtual void ReapplyCustomPropertiesTo(IgdbGameReference target)
        {
            target.NameOverride = this.NameOverride;
            target.NameAppendReleaseYear = this.NameAppendReleaseYear;
            target.FirstReleaseYear = this.FirstReleaseYear;
            target.NameAppendReleasePlatforms = this.NameAppendReleasePlatforms;
            target.ReleasePlatformNames = this.ReleasePlatformNames;
            target.NameAppendReleaseNumber = this.NameAppendReleaseNumber;
            target.ReleaseNumber = this.ReleaseNumber;
        }
    }
}
