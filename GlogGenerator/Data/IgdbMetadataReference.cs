using System;
using System.Collections.Generic;
using GlogGenerator.IgdbApi;
using Newtonsoft.Json;

namespace GlogGenerator.Data
{
    public class IgdbMetadataReference : IgdbEntityReference<IgdbEntity>, IIgdbEntityReference
    {
        [JsonProperty("igdbMetadataName")]
        public string Name { get; private set; }

        [JsonProperty("nameOverride")]
        public string NameOverride { get; private set; } = null;

        public IgdbMetadataReference() : base() { }

        public IgdbMetadataReference(string inNameOverride) : base()
        {
            this.NameOverride = inNameOverride;
        }

        public IgdbMetadataReference(IgdbEntity fromEntity) : base(fromEntity)
        {
            var entityType = fromEntity.GetType();
            if (entityType == typeof(IgdbCollection))
            {
                this.Name = (fromEntity as IgdbCollection).Name;
            }
            else if (entityType == typeof(IgdbCompany))
            {
                this.Name = (fromEntity as IgdbCompany).Name;
            }
            else if (entityType == typeof(IgdbFranchise))
            {
                this.Name = (fromEntity as IgdbFranchise).Name;
            }
            else if (entityType == typeof(IgdbGameMode))
            {
                this.Name = (fromEntity as IgdbGameMode).Name;
            }
            else if (entityType == typeof(IgdbGameType))
            {
                this.Name = (fromEntity as IgdbGameType).Type;
            }
            else if (entityType == typeof(IgdbGenre))
            {
                this.Name = (fromEntity as IgdbGenre).Name;
            }
            else if (entityType == typeof(IgdbKeyword))
            {
                this.Name = (fromEntity as IgdbKeyword).Name;
            }
            else if (entityType == typeof(IgdbPlayerPerspective))
            {
                this.Name = (fromEntity as IgdbPlayerPerspective).Name;
            }
            else if (entityType == typeof(IgdbTheme))
            {
                this.Name = (fromEntity as IgdbTheme).Name;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override string GetReferenceableKey()
        {
            if (!string.IsNullOrEmpty(this.NameOverride))
            {
                return this.NameOverride;
            }

            return Name;
        }
    }
}
