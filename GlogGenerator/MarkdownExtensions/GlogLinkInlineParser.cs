using System;
using System.Collections.Generic;
using System.IO;
using GlogGenerator.Data;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogLinkInlineParser : LinkInlineParser
    {
        private readonly ISiteDataIndex siteDataIndex;

        private Dictionary<string, string> linkMatchTypes;

        public GlogLinkInlineParser(ISiteDataIndex siteDataIndex) : base()
        {
            this.siteDataIndex = siteDataIndex;

            this.linkMatchTypes = new Dictionary<string, string>();
            foreach (var linkHandler in GlogLinkHandlers.LinkMatchHandlers)
            {
                var linkMatchString = $"]({linkHandler.Key}:";
                this.linkMatchTypes.Add(linkMatchString, linkHandler.Key);
            }
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            if (slice.CurrentChar == ']')
            {
                foreach (var linkMatchHandler in this.linkMatchTypes)
                {
                    var linkMatchString = linkMatchHandler.Key;
                    if (slice.Length > linkMatchString.Length && slice.Text.Substring(slice.Start, linkMatchString.Length).Equals(linkMatchString, StringComparison.OrdinalIgnoreCase))
                    {
                        var openParent = processor.Inline!.FirstParentOfType<LinkDelimiterInline>();
                        if (openParent is null)
                        {
                            return false;
                        }

                        slice.Start += linkMatchString.Length;

                        var referenceEndPos = slice.Start;
                        var openParens = 0;
                        while (referenceEndPos < slice.End)
                        {
                            referenceEndPos = slice.Text.IndexOfAny(new char[] { '(', ')' }, referenceEndPos);
                            if (referenceEndPos < 0)
                            {
                                throw new InvalidDataException($"Failed to parse link in {slice}");
                            }

                            if (slice.Text[referenceEndPos] == '(')
                            {
                                ++openParens;
                                ++referenceEndPos;
                            }
                            else // Must be a ')'
                            {
                                if (openParens > 0)
                                {
                                    --openParens;
                                    ++referenceEndPos;
                                }
                                else // No more open parens, so we're done!
                                {
                                    break;
                                }
                            }
                        }

                        var referenceTypeName = linkMatchHandler.Value;
                        var referenceKey = slice.Text.Substring(slice.Start, referenceEndPos - slice.Start);

                        var glogLinkInline = new GlogLinkInline(referenceTypeName, referenceKey, this.siteDataIndex)
                        {
                            // TODO?: would filling in Span, Row, and Column accomplish anything?
                        };

                        // Processing state management, adapted from `LinkInlineParser.TryProcessLinkOrImage`
                        // https://github.com/xoofx/markdig/blob/master/src/Markdig/Parsers/Inlines/LinkInlineParser.cs
                        openParent.ReplaceBy(glogLinkInline);
                        processor.Inline = glogLinkInline;
                        processor.PostProcessInlines(0, glogLinkInline, null, false);
                        openParent.IsActive = false;
                        glogLinkInline.IsClosed = true;

                        slice.Start = referenceEndPos + 1;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
