using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace GlogGenerator.MarkdownExtensions
{
    public static class IMarkdownObjectExtensions
    {
        private static readonly object ReversibleGenericAttributesKey = typeof(ReversibleGenericAttributes);
        private static readonly object RoundtripOriginalSourceKey = typeof(RoundtripOriginalSource);

        public static ReversibleGenericAttributes TryGetReversibleGenericAttributes(this IMarkdownObject obj)
        {
            return obj.GetData(ReversibleGenericAttributesKey) as ReversibleGenericAttributes;
        }

        public static RoundtripOriginalSource TryGetRoundtripOriginalSource(this IMarkdownObject obj)
        {
            return obj.GetData(RoundtripOriginalSourceKey) as RoundtripOriginalSource;
        }

        public static void SetReversibleGenericAttributes(this IMarkdownObject obj, ReversibleGenericAttributes attributes)
        {
            obj.SetData(ReversibleGenericAttributesKey, attributes);
        }

        public static void SetRoundtripOriginalSource(this IMarkdownObject obj, RoundtripOriginalSource source)
        {
            obj.SetData(RoundtripOriginalSourceKey, source);
        }
    }
}
