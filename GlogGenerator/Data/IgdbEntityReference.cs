using System;
using GlogGenerator.IgdbApi;
using Newtonsoft.Json;

namespace GlogGenerator.Data
{
    public abstract class IgdbEntityReference<T> where T : IgdbEntity
    {
        [JsonProperty("igdbEntityType")]
        public string IgdbType { get; private set; } = null;

        [JsonProperty("igdbEntityId")]
        public int? IgdbId { get; private set; } = null;

        public IgdbEntityReference(T fromEntity)
        {
            if (fromEntity.GetEntityId() != IgdbEntity.IdNotFound)
            {
                this.IgdbType = fromEntity.GetType().Name;
                this.IgdbId = fromEntity.GetEntityId();
            }
        }

        public virtual string GetReferenceableKey()
        {
            throw new NotImplementedException();
        }
    }
}
