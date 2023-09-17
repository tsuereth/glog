using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class SpoilerParser : InlineParser
    {
        public SpoilerParser()
        {
            this.OpeningCharacters = new char[] { '>', '!' };
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            if (slice.CurrentChar == '>' && slice.PeekChar() == '!')
            {
                this.IncrementNestedCount(processor);

                slice.SkipChar(); // >
                slice.SkipChar(); // !

                processor.Inline = SpoilerInline.BeginSpoiler();
                return true;
            }
            else if (slice.CurrentChar == '!' && slice.PeekChar() == '<')
            {
                if (!this.IsCurrentlyNested(processor))
                {
                    return false;
                }

                // Unfortunately, it's very easy to catch false-positives when
                // an exciting sentence is followed by a closing tag:
                // Like a spoiler about >!some <b>really cool thing!</b>!<
                // So we bail out of this parser condition if:
                // - There's another "!<" after this one, and
                // - No additional ">!" inbetween here and there.
                var lookaheadEndTokenPos = slice.IndexOf("!<", 2);
                if (lookaheadEndTokenPos >= 0)
                {
                    var lookaheadStartTokenPos = slice.IndexOf(">!", 2);
                    if (lookaheadStartTokenPos < 0 || lookaheadStartTokenPos > lookaheadEndTokenPos)
                    {
                        return false;
                    }
                }

                this.DecrementNestedCount(processor);

                slice.SkipChar(); // !
                slice.SkipChar(); // <

                processor.Inline = SpoilerInline.EndSpoiler();
                return true;
            }

            return false;
        }

        private void IncrementNestedCount(InlineProcessor processor)
        {
            if (processor.ParserStates[this.Index] == null)
            {
                processor.ParserStates[this.Index] = new SpoilerState();
            }

            (processor.ParserStates[this.Index] as SpoilerState).IncrementNestedCount();
        }

        private bool IsCurrentlyNested(InlineProcessor processor)
        {
            if (processor.ParserStates[this.Index] == null)
            {
                return false;
            }

            return ((processor.ParserStates[this.Index] as SpoilerState).NestedCount > 0);
        }

        private void DecrementNestedCount(InlineProcessor processor)
        {
            // Is the state null? is the count already 0? great, throw an exception about it!
            (processor.ParserStates[this.Index] as SpoilerState).DecrementNestedCount();
        }

        private sealed class SpoilerState
        {
            public uint NestedCount { get; set; }

            public void IncrementNestedCount()
            {
                ++this.NestedCount;
            }

            public void DecrementNestedCount()
            {
                --this.NestedCount;
            }
        }
    }
}
