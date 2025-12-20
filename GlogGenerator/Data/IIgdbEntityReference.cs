using System;
using System.IO;
using GlogGenerator.IgdbApi;

namespace GlogGenerator.Data
{
    public interface IIgdbEntityReference
    {
        public bool HasIgdbEntityData();

        public static string FormatIgdbEntityReferenceDataId(Type igdbEntityType, int igdbEntityId)
        {
            return $"{igdbEntityType.Name}:id={igdbEntityId}";
        }

        public static string GetIgdbEntityReferenceDataId(IgdbEntity igdbEntity)
        {
            if (igdbEntity == null || igdbEntity.GetEntityId() == IgdbEntity.IdNotFound)
            {
                throw new InvalidDataException();
            }

            return FormatIgdbEntityReferenceDataId(igdbEntity.GetType(), igdbEntity.GetEntityId());
        }

        public string GetIgdbEntityDataId();

        public string GetReferenceableKey();
    }
}
