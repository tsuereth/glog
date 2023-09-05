using System.ComponentModel;
using System.Reflection;

namespace GlogGenerator.IgdbApi
{
    public static class IgdbGameCategoryExtensions
    {
        public static string Description(this IgdbGameCategory category)
        {
            var categoryEnumInfo = typeof(IgdbGameCategory).GetMember(category.ToString())[0];
            var categoryDescriptionAttr = categoryEnumInfo.GetCustomAttribute<DescriptionAttribute>();
            return categoryDescriptionAttr.Description;
        }
    }
}
