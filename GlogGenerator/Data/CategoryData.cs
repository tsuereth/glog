using System.Collections.Generic;
using GlogGenerator.TemplateRenderers;

namespace GlogGenerator.Data
{
    public class CategoryData : IGlogReferenceable
    {
        public string Name { get; set; } = string.Empty;

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();

        public string GetPermalinkRelative()
        {
            var urlized = StringRenderer.Urlize(this.Name);
            return $"category/{urlized}/";
        }
    }
}
