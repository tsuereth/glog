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
                var urlized = TemplateFunctionsStringRenderer.Urlize(this.Name, htmlEncode: true);
                return $"category/{urlized}/";
            }
        }

        public string OutputDirRelative
        {
            get
            {
                var urlizedDir = TemplateFunctionsStringRenderer.Urlize(this.Name, htmlEncode: false, terminologySpecial: true);
                return $"category/{urlizedDir}";
            }
        }

        public string Name { get; set; } = string.Empty;

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();
    }
}
