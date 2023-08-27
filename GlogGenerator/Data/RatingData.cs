using System.Collections.Generic;
using System.IO;
using GlogGenerator.HugoCompat;

namespace GlogGenerator.Data
{
    public class RatingData
    {
        public string PermalinkRelative
        {
            get
            {
                var urlized = TemplateFunctionsStringRenderer.Urlize(this.Name, htmlEncode: true);
                return $"rating/{urlized}/";
            }
        }

        public string OutputDirRelative
        {
            get
            {
                var urlizedDir = TemplateFunctionsStringRenderer.Urlize(this.Name, htmlEncode: false, terminologySpecial: true);
                var urlizedDirParts = urlizedDir.Split('/');
                return Path.Combine("rating", Path.Combine(urlizedDirParts));
            }
        }

        public string Name { get; set; } = string.Empty;

        public List<PostData> LinkedPosts { get; set; } = new List<PostData>();
    }
}
