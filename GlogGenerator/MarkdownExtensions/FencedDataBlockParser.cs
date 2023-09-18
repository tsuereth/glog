using System.Text;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace GlogGenerator.MarkdownExtensions
{
    public class FencedDataBlockParser : FencedBlockParserBase<FencedDataBlock>
    {
        public FencedDataBlockParser() : base()
        {
            OpeningCharacters = new char[] { ':' };
        }

        protected override FencedDataBlock CreateFencedBlock(BlockProcessor processor)
        {
            return new FencedDataBlock(this);
        }

        public override BlockState TryContinue(BlockProcessor processor, Block block)
        {
            var result = base.TryContinue(processor, block);
            if (result == BlockState.Continue)
            {
                var slice = processor.Line;

                var dataKeyBuilder = new StringBuilder();
                var hitKvSeparator = false;
                var dataValueBuilder = new StringBuilder();
                var c = slice.CurrentChar;
                while (c != '\0')
                {
                    if (!hitKvSeparator && c == ':')
                    {
                        hitKvSeparator = true;
                    }
                    else if (!hitKvSeparator)
                    {
                        dataKeyBuilder.Append(c);
                    }
                    else
                    {
                        dataValueBuilder.Append(c);
                    }

                    c = slice.NextChar();
                }

                if (dataValueBuilder.Length > 0)
                {
                    var dataKey = dataKeyBuilder.ToString();
                    var dataValue = dataValueBuilder.ToString();

                    var dataBlock = block as FencedDataBlock;
                    dataBlock.Data[dataKey.Trim()] = dataValue.Trim();
                }
            }

            return result;
        }
    }
}
