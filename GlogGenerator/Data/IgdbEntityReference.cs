using System;
using System.IO;
using GlogGenerator.IgdbApi;
using GlogGenerator.NewtonsoftJsonHelpers;
using Newtonsoft.Json;

namespace GlogGenerator.Data
{
    public abstract class IgdbEntityReference<T> : IIgdbEntityReference
        where T : IgdbEntity
    {
        [JsonConverter(typeof(StringTypeNameConverter), "GlogGenerator", "GlogGenerator.IgdbApi")]
        [JsonProperty("igdbEntityType")]
        public Type IgdbEntityType { get; private set; } = null;

        [JsonProperty("igdbEntityId")]
        public int? IgdbEntityId { get; private set; } = null;

        public IgdbEntityReference() { }

        public IgdbEntityReference(T fromEntity)
        {
            if (fromEntity.GetEntityId() != IgdbEntity.IdNotFound)
            {
                this.IgdbEntityType = fromEntity.GetType();
                this.IgdbEntityId = fromEntity.GetEntityId();
            }
        }

        public bool HasIgdbEntityData()
        {
            return this.IgdbEntityId.HasValue && this.IgdbEntityId.Value != IgdbEntity.IdNotFound;
        }

        public string GetIgdbEntityDataId()
        {
            if (!this.HasIgdbEntityData())
            {
                throw new InvalidDataException();
            }

            return IIgdbEntityReference.FormatIgdbEntityReferenceDataId(this.IgdbEntityType, this.IgdbEntityId.Value);
        }

        public virtual string GetReferenceableKey()
        {
            throw new NotImplementedException();
        }
    }
}
