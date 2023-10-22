using System;
using System.Linq;
using System.Reflection;

namespace GlogGenerator.IgdbApi
{
    public abstract class IgdbEntity
    {
        public const int IdNotFound = -1;

        private readonly PropertyInfo idProperty;
        private readonly PropertyInfo referenceableKeyProperty;

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

            var referenceableKeyProperties = entityType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(IgdbEntityReferenceableKeyAttribute)));
            if (referenceableKeyProperties.Any())
            {
                if (referenceableKeyProperties.Count() > 1)
                {
                    throw new AmbiguousMatchException($"The {entityType} type has too many properties with {nameof(IgdbEntityReferenceableKeyAttribute)}");
                }

                this.referenceableKeyProperty = referenceableKeyProperties.First();
                if (this.referenceableKeyProperty.PropertyType != typeof(string))
                {
                    throw new InvalidCastException($"The {entityType} type {nameof(IgdbEntityReferenceableKeyAttribute)} {this.referenceableKeyProperty.Name} is not of the correct `string` type");
                }
            }
            else
            {
                this.referenceableKeyProperty = null;
            }
        }

        public int GetEntityId()
        {
            var idObject = this.idProperty.GetValue(this);
            return (int)idObject;
        }

        public string GetReferenceableKey()
        {
            if (this.referenceableKeyProperty != null)
            {
                var referenceableKeyObject = this.referenceableKeyProperty.GetValue(this);
                return (string)referenceableKeyObject;
            }
            else
            {
                return null;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class IgdbEntityIdAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class IgdbEntityReferenceableKeyAttribute : Attribute { }
}
