using System;
using System.Collections.Generic;

namespace GlogGenerator.Data
{
    public interface IGlogMultiKeyReferenceable : IGlogReferenceable
    {
        public void MergeReferenceableKey(Type mergeKeyType, string mergeKey);
    }
}
