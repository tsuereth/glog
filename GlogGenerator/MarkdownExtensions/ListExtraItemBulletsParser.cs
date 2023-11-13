using Markdig.Extensions.ListExtras;
using Markdig.Helpers;
using Markdig.Parsers;

namespace GlogGenerator.MarkdownExtensions
{
    public class ListExtraItemBulletParser : ListExtraItemParser
    {
        public ListExtraItemBulletParser() : base() { }

        public override bool TryParse(BlockProcessor state, char pendingBulletType, out ListInfo result)
        {
            var originalSlice = state.Line;
            var parsed = base.TryParse(state, pendingBulletType, out result);

            if (parsed)
            {
                // Bugfix: set the list item's SourceBullet so that it can be roundtrip rendered.
                var sourceBullet = new StringSlice(originalSlice.Text, originalSlice.Start, state.Start - 1);
                if (sourceBullet.PeekCharAbsolute(sourceBullet.End) == result.OrderedDelimiter)
                {
                    sourceBullet.End--;
                }

                result.SourceBullet = sourceBullet;
            }

            return parsed;
        }
    }
}
