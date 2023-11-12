using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace GlogGenerator.MarkdownExtensions
{
    public static class IMarkdownObjectExtensions
    {
        private static readonly object ReversibleGenericAttributesKey = typeof(ReversibleGenericAttributes);

        public static ReversibleGenericAttributes TryGetReversibleGenericAttributes(this IMarkdownObject obj)
        {
            return obj.GetData(ReversibleGenericAttributesKey) as ReversibleGenericAttributes;
        }

        public static void SetReversibleGenericAttributes(this IMarkdownObject obj, ReversibleGenericAttributes attributes)
        {
            obj.SetData(ReversibleGenericAttributesKey, attributes);
        }
    }
}
