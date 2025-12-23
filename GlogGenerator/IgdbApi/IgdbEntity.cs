using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GlogGenerator.Data;

namespace GlogGenerator.IgdbApi
{
    public abstract class IgdbEntity
    {
        public const int IdNotFound = -1;

        private readonly PropertyInfo idProperty;

        private bool forcePersistInCache = false;

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
        }

        public virtual bool ShouldForcePersistInCache()
        {
            return this.forcePersistInCache;
        }

        public void SetForcePersistInCache()
        {
            this.forcePersistInCache = true;
        }

        public int GetEntityId()
        {
            var idObject = this.idProperty.GetValue(this);
            return (int)idObject;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class IgdbEntityIdAttribute : Attribute { }
}
