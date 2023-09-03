using System.Collections.Generic;
using GlogGenerator.HugoCompat;

namespace GlogGenerator.Data
{
    public class PlatformData
    {
        public string PermalinkRelative
        {
            get
            {
                var urlized = TemplateFunctionsStringRenderer.Urlize(this.Name, htmlEncode: true);
                return $"platform/{urlized}/";
            }
        }

        public string OutputDirRelative
        {
            get
            {
                var urlizedDir = TemplateFunctionsStringRenderer.Urlize(this.Name, htmlEncode: false, terminologySpecial: true);
                return $"platform/{urlizedDir}";
            }
        }

        public string Name { get; set; } = string.Empty;

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();
    }
}
