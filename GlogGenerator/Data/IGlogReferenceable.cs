namespace GlogGenerator.Data
{
    public interface IGlogReferenceable
    {
        public string GetDataId();

        public string GetReferenceableKey();

        public bool MatchesReferenceableKey(string matchKey);

        public string GetPermalinkRelative();
    }
}
