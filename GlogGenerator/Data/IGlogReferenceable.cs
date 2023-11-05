namespace GlogGenerator.Data
{
    public interface IGlogReferenceable
    {
        public string GetDataId();

        public string GetReferenceableKey();

        public string GetPermalinkRelative();
    }
}
