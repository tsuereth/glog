﻿using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly Dictionary<string, PropertyInfo> overrideValueProperties;

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

            this.overrideValueProperties = entityType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(IgdbEntityGlogOverrideValueAttribute))).ToDictionary(p => p.Name, p => p);
        }

        public int GetEntityId()
        {
            var idObject = this.idProperty.GetValue(this);
            return (int)idObject;
        }

        public string GetUniqueIdString(IIgdbCache cache)
        {
            var entityType = this.GetType();

            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var typeBytes = Encoding.UTF8.GetBytes(entityType.Name);
                hash.AppendData(typeBytes);

                var entityId = this.GetEntityId();
                if (entityId != IdNotFound)
                {
                    var entityIdBytes = BitConverter.GetBytes(entityId);
                    hash.AppendData(entityIdBytes);
                }
                else
                {
                    var referenceString = this.GetReferenceString(cache);
                    if (string.IsNullOrEmpty(referenceString))
                    {
                        throw new InvalidDataException($"An {entityType} is unable to generate a unique ID string because it has no Entity ID and no Reference String");
                    }

                    var keyBytes = Encoding.UTF8.GetBytes(referenceString);
                    hash.AppendData(keyBytes);
                }

                var idStringBytes = hash.GetCurrentHash();
                return Convert.ToHexString(idStringBytes);
            }
        }

        public virtual string GetReferenceString(IIgdbCache cache)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetGlogOverrideValues()
        {
            var overrideValues = new Dictionary<string, object>();
            foreach (var propertyName in this.overrideValueProperties.Keys)
            {
                var property = this.overrideValueProperties[propertyName];
                overrideValues[propertyName] = property.GetValue(this);
            }

            return overrideValues;
        }

        public void SetGlogOverrideValues(Dictionary<string, object> overrideValues)
        {
            var entityType = this.GetType();

            foreach (var propertyName in overrideValues.Keys)
            {
                if (this.overrideValueProperties.TryGetValue(propertyName, out var overrideProperty))
                {
                    overrideProperty.SetValue(this, overrideValues[propertyName]);
                }
                else
                {
                    throw new AmbiguousMatchException($"The {entityType} type doesn't have an override property named {propertyName}");
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class IgdbEntityIdAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class IgdbEntityGlogOverrideValueAttribute : Attribute { }
}
