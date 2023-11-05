using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using GlogGenerator.Data;

namespace GlogGenerator.IgdbApi
{
    public abstract class IgdbEntity
    {
        public const int IdNotFound = -1;

        private readonly PropertyInfo idProperty;
        private readonly PropertyInfo referenceableValueProperty;

        public IgdbEntity()
        {
            var entityType = this.GetType();

            var idProperties = entityType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(IgdbEntityIdAttribute)));
            if (!idProperties.Any())
            {
                throw new NotImplementedException($"The {entityType} type is missing a property with {nameof(IgdbEntityIdAttribute)}");
            }
            else if (idProperties.Count() > 1)
            {
                throw new AmbiguousMatchException($"The {entityType} type has too many properties with {nameof(IgdbEntityIdAttribute)}");
            }

            this.idProperty = idProperties.First();
            if (this.idProperty.PropertyType != typeof(int))
            {
                throw new InvalidCastException($"The {entityType} type {nameof(IgdbEntityIdAttribute)} {this.idProperty.Name} is not of the correct `int` type");
            }

            var referenceableValueProperties = entityType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(IgdbEntityReferenceableValueAttribute)));
            if (referenceableValueProperties.Any())
            {
                if (referenceableValueProperties.Count() > 1)
                {
                    throw new AmbiguousMatchException($"The {entityType} type has too many properties with {nameof(IgdbEntityReferenceableValueAttribute)}");
                }

                this.referenceableValueProperty = referenceableValueProperties.First();
                if (this.referenceableValueProperty.PropertyType != typeof(string))
                {
                    throw new InvalidCastException($"The {entityType} type {nameof(IgdbEntityReferenceableValueAttribute)} {this.referenceableValueProperty.Name} is not of the correct `string` type");
                }
            }
            else
            {
                this.referenceableValueProperty = null;
            }
        }

        public int GetEntityId()
        {
            var idObject = this.idProperty.GetValue(this);
            return (int)idObject;
        }

        public string GetUniqueIdString()
        {
            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var typeBytes = Encoding.UTF8.GetBytes(nameof(GameData));
                hash.AppendData(typeBytes);

                var entityId = this.GetEntityId();
                if (entityId != IdNotFound)
                {
                    var entityIdBytes = BitConverter.GetBytes(entityId);
                    hash.AppendData(entityIdBytes);
                }
                else
                {
                    var keyBytes = Encoding.UTF8.GetBytes(this.GetReferenceableValue());
                    hash.AppendData(keyBytes);
                }

                var idStringBytes = hash.GetCurrentHash();
                return Convert.ToHexString(idStringBytes);
            }
        }

        public string GetReferenceableValue()
        {
            if (this.referenceableValueProperty != null)
            {
                var referenceableKeyObject = this.referenceableValueProperty.GetValue(this);
                return (string)referenceableKeyObject;
            }
            else
            {
                return null;
            }
        }

        public string GetReferenceableKey()
        {
            return UrlizedString.Urlize(this.GetReferenceableValue());
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class IgdbEntityIdAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class IgdbEntityReferenceableValueAttribute : Attribute { }
}
