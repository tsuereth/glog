using System.Collections.Generic;

namespace GlogGenerator.Data
{
    public class TagData : IGlogReferenceable
    {
        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.Name);
            return $"tag/{urlized}/";
        }

        public string Name { get; set; } = string.Empty;

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();
    }
}
