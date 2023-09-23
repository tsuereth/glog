using System.Collections.Generic;
using GlogGenerator.TemplateRenderers;

namespace GlogGenerator.Data
{
    public class CategoryData
    {
        public string PermalinkRelative
        {
            get
            {
                var urlized = StringRenderer.Urlize(this.Name);
                return $"category/{urlized}/";
            }
        }

        public string Name { get; set; } = string.Empty;

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();
    }
}
