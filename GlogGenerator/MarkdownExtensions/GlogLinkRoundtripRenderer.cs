﻿using System.Text;
using GlogGenerator.Data;
using Markdig.Renderers;
using Markdig.Renderers.Roundtrip;
using Markdig.Syntax.Inlines;

namespace GlogGenerator.MarkdownExtensions
{
    public class GlogLinkRoundtripRenderer : RoundtripObjectRenderer<GlogLinkInline>
    {
        private readonly ISiteDataIndex siteDataIndex;

        public GlogLinkRoundtripRenderer(ISiteDataIndex siteDataIndex) : base()
        {
            this.siteDataIndex = siteDataIndex;
        }

        protected override void Write(RoundtripRenderer renderer, GlogLinkInline obj)
        {
            var stringBuilder = new StringBuilder();
            if (obj.IsAutoLink)
            {
                stringBuilder.Append('<');
                stringBuilder.Append(obj.ReferenceTypeName);
                stringBuilder.Append(':');
                stringBuilder.Append(obj.GetReferenceKey(this.siteDataIndex));
                stringBuilder.Append('>');
            }
            else
            {
                renderer.Write('[');
                renderer.WriteChildren(obj);

                stringBuilder.Append("](");
                stringBuilder.Append(obj.ReferenceTypeName);
                stringBuilder.Append(':');
                stringBuilder.Append(obj.GetReferenceKey(this.siteDataIndex));
                stringBuilder.Append(')');
            }

            renderer.Write(stringBuilder.ToString());
        }
    }
}
