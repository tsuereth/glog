using System;

namespace GlogGenerator.Data
{
    public class SiteDataReference<T> : ISiteDataReference
        where T : IGlogReferenceable
    {
        private string unresolvedReferenceKey = null;
        private string resolvedReferenceId = null;

        public SiteDataReference(string referenceKey)
        {
            this.unresolvedReferenceKey = referenceKey;
        }

        public bool GetIsResolved()
        {
            return !string.IsNullOrEmpty(resolvedReferenceId);
        }

        public string GetUnresolvedReferenceKey()
        {
            return this.unresolvedReferenceKey;
        }

        public string GetResolvedReferenceId()
        {
            return this.resolvedReferenceId;
        }

        public void SetData(IGlogReferenceable data)
        {
            this.resolvedReferenceId = data.GetDataId();
            if (string.IsNullOrEmpty(this.resolvedReferenceId))
            {
                throw new ArgumentException("Data is missing an ID");
            }
            
            this.unresolvedReferenceKey = null;
        }
    }
}
