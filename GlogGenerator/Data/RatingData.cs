using System.Collections.Generic;

namespace GlogGenerator.Data
{
    public class RatingData : IGlogReferenceable
    {
        public string Name { get; set; } = string.Empty;

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        public string GetPermalinkRelative()
        {
            var urlized = UrlizedString.Urlize(this.Name);
            return $"rating/{urlized}/";
        }
    }
}
