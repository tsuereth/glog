using System.Collections.Generic;
using GlogGenerator.HugoCompat;

namespace GlogGenerator.Data
{
    public class CategoryData
    {
        public string PermalinkRelative
        {
            get
            {
                var urlized = TemplateFunctionsStringRenderer.Urlize(this.Name);
                return $"category/{urlized}/";
            }
        }

        public string Name { get; set; } = string.Empty;

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();
    }
}
