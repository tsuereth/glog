using System;

namespace GlogGenerator.DiffSummary
{
    public class DiffLine
    {
        public enum DiffLineType
        {
            Added,
            Removed,
            Unchanged,
        }

        public DiffLineType DiffType { get; private set; }

        public string Line { get; private set; }

        public DiffLine(DiffLineType diffType, string line)
        {
            this.DiffType = diffType;
            this.Line = line;
        }

        public char GetDiffTypeChar()
        {
            switch (this.DiffType)
            {
                case DiffLineType.Added:
                    return '+';
                case DiffLineType.Removed:
                    return '-';
                case DiffLineType.Unchanged:
                    return ' ';
            }

            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{this.GetDiffTypeChar()} {this.Line}";
        }

        public bool Equals(DiffLine otherDiffLine)
        {
            return this.DiffType == otherDiffLine.DiffType && this.Line.Equals(otherDiffLine.Line, StringComparison.Ordinal);
        }
    }
}
