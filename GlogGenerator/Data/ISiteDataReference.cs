namespace GlogGenerator.Data
{
    public interface ISiteDataReference
    {
        public bool GetIsResolved();

        public string GetUnresolvedReferenceKey();

        public string GetResolvedReferenceId();

        public void SetData(IGlogReferenceable data);
    }
}
