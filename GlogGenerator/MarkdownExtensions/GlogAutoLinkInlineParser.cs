using System;
using System.Collections.Generic;
using System.IO;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogAutoLinkInlineParser : AutolinkInlineParser
    {
        private Dictionary<string, string> linkMatchTypes;

        public GlogAutoLinkInlineParser() : base()
        {
            this.linkMatchTypes = new Dictionary<string, string>();
            foreach (var linkHandler in GlogLinkHandlers.LinkMatchHandlers)
            {
                var linkMatchString = $"<{linkHandler.Key}:";
                this.linkMatchTypes.Add(linkMatchString, linkHandler.Key);
            }
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var matchResult = false;

            if (slice.CurrentChar == '<')
            {
                foreach (var linkMatchHandler in this.linkMatchTypes)
                {
                    var linkMatchString = linkMatchHandler.Key;
                    if (slice.Length > linkMatchString.Length && slice.Text.Substring(slice.Start, linkMatchString.Length).Equals(linkMatchString, StringComparison.OrdinalIgnoreCase))
                    {
                        var saved = slice;

                        slice.Start += linkMatchString.Length;

                        var referenceEndPos = slice.Start;
                        var openAngleBraces = 0;
                        while (referenceEndPos < slice.End)
                        {
                            referenceEndPos = slice.Text.IndexOfAny(new char[] { '<', '>' }, referenceEndPos);
                            if (referenceEndPos < 0)
                            {
                                throw new InvalidDataException($"Failed to parse link in {slice}");
                            }

                            if (slice.Text[referenceEndPos] == '<')
                            {
                                ++openAngleBraces;
                                ++referenceEndPos;
                            }
                            else // Must be a '>'
                            {
                                if (openAngleBraces > 0)
                                {
                                    --openAngleBraces;
                                    ++referenceEndPos;
                                }
                                else // No more open braces, so we're done!
                                {
                                    break;
                                }
                            }
                        }

                        var referenceType = linkMatchHandler.Value;
                        var referenceKey = slice.Text.Substring(slice.Start, referenceEndPos - slice.Start);

                        var glogLinkInline = new GlogLinkInline()
                        {
                            ReferenceType = referenceType,
                            ReferenceKey = referenceKey,
                            // TODO?: would filling in Span, Row, and Column accomplish anything?
                        };
                        glogLinkInline.AppendChild(new LiteralInline(referenceKey));
                        glogLinkInline.IsClosed = true;

                        processor.Inline = glogLinkInline;

                        slice.Start = referenceEndPos + 1;
                        matchResult = true;
                        break;
                    }
                }
            }

            // If this parser didn't detect a special link, fall-back to the builtin parser.
            if (!matchResult)
            {
                matchResult = base.Match(processor, ref slice);
            }

            return matchResult;
        }
    }
}
