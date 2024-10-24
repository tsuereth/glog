using System;
using System.Collections.Generic;
using System.IO;
using GlogGenerator.Data;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogAutoLinkInlineParser : AutolinkInlineParser
    {
        private readonly ISiteDataIndex siteDataIndex;

        private Dictionary<string, string> linkMatchTypes;

        public GlogAutoLinkInlineParser(ISiteDataIndex siteDataIndex) : base()
        {
            this.siteDataIndex = siteDataIndex;

            this.linkMatchTypes = new Dictionary<string, string>();
            foreach (var linkHandler in GlogLinkHandlers.LinkMatchHandlers)
            {
                var linkMatchString = $"<{linkHandler.Key}:";
                this.linkMatchTypes.Add(linkMatchString, linkHandler.Key);
            }
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
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

                        var referenceTypeName = linkMatchHandler.Value;
                        var referenceKey = slice.Text.Substring(slice.Start, referenceEndPos - slice.Start);

                        var linkDataIndex = processor.Context.UseSiteDataIndex() ? this.siteDataIndex : null;
                        var glogLinkInline = new GlogLinkInline(referenceTypeName, referenceKey, linkDataIndex)
                        {
                            IsAutoLink = true,
                            // TODO?: would filling in Span, Row, and Column accomplish anything?
                        };
                        glogLinkInline.AppendChild(new LiteralInline(referenceKey));
                        glogLinkInline.IsClosed = true;

                        processor.Inline = glogLinkInline;

                        slice.Start = referenceEndPos + 1;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
