using System;
using System.Collections.Generic;
using System.IO;
using GlogGenerator.HugoCompat;
using GlogGenerator.RenderState;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogLinkInlineParser : LinkInlineParser
    {
        private readonly SiteState siteState;

        private Dictionary<string, Func<SiteState, string, string>> linkMatchHandlers;

        public GlogLinkInlineParser(
            SiteState siteState)
            : base()
        {
            this.siteState = siteState;

            this.linkMatchHandlers = new Dictionary<string, Func<SiteState, string, string>>();
            foreach (var linkHandler in GlogLinkHandlers.LinkMatchHandlers)
            {
                var linkMatchString = $"]({linkHandler.Key}:";
                this.linkMatchHandlers.Add(linkMatchString, linkHandler.Value);
            }
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var matchResult = false;

            if (slice.CurrentChar == ']')
            {
                foreach (var linkMatchHandler in this.linkMatchHandlers)
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

                        var referenceName = slice.Text.Substring(slice.Start, referenceEndPos - slice.Start);

                        var referenceLink = linkMatchHandler.Value(this.siteState, referenceName);

                        var linkInline = new LinkInline()
                        {
                            Url = referenceLink,
                            // TODO?: would filling in Span, Row, and Column accomplish anything?
                        };

                        // Processing state management, adapted from `LinkInlineParser.TryProcessLinkOrImage`
                        // https://github.com/xoofx/markdig/blob/master/src/Markdig/Parsers/Inlines/LinkInlineParser.cs
                        openParent.ReplaceBy(linkInline);
                        processor.Inline = linkInline;
                        processor.PostProcessInlines(0, linkInline, null, false);
                        openParent.IsActive = false;
                        linkInline.IsClosed = true;

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
