using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace GlogGenerator.Data
{
    public class RatingData : IGlogReferenceable
    {
        public string Name { get; set; } = string.Empty;

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        public string GetDataId()
        {
            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var typeBytes = Encoding.UTF8.GetBytes(nameof(RatingData));
                hash.AppendData(typeBytes);

                var nameBytes = Encoding.UTF8.GetBytes(this.Name);
                hash.AppendData(nameBytes);

                var idBytes = hash.GetCurrentHash();
                return Convert.ToHexString(idBytes);
            }
        }

        public string GetReferenceableKey()
        {
            return UrlizedString.Urlize(this.Name);
        }

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.Name);
            return $"rating/{urlized}/";
        }
    }
}
