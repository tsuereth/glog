using System.Collections.Generic;

namespace GlogGenerator.Data
{
    public interface IGlogReferenceable
    {
        public string GetDataId();

        public string GetReferenceableKey();

        public bool MatchesReferenceableKey(string matchKey);

        public IEnumerable<string> GetIgdbEntityReferenceIds();

        public object GetReferenceProperties();

        public string GetPermalinkRelative();
    }
}
