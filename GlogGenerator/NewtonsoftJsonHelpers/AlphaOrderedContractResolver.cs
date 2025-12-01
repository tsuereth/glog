using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GlogGenerator.NewtonsoftJsonHelpers
{
    public class AlphaOrderedContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var serializedProperties = base.CreateProperties(type, memberSerialization);
            return serializedProperties.OrderBy(p => p.Order ?? 0).ThenBy(p => p.PropertyName).ToList();
        }
    }
}
