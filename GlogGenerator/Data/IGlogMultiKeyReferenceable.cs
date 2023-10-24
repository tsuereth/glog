namespace GlogGenerator.Data
{
    public interface IGlogMultiKeyReferenceable : IGlogReferenceable
    {
        public void MergeReferenceableKey(string mergeKey);
    }
}
