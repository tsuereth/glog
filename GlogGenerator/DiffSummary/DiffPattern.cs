using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DiffPlex.Model;
using DiffPlex.Renderer;

namespace GlogGenerator.DiffSummary
{
    public class DiffPattern
    {
        public DiffFilePaths FilePaths { get; private set; }

        public string FirstFilePath { get; private set; }

        public int OldStartLine { get; private set; }

        public int OldLength { get; private set; }

        public int NewStartLine { get; private set; }

        public int NewLength { get; private set; }

        public string[] LinesBeforeDiff { get; private set; }

        public DiffLine[] DiffLines { get; private set; }

        public string[] LinesAfterDiff { get; private set; }

        public DiffPattern(string firstFilePath, int oldStartLine, int oldLength, int newStartLine, int newLength, string[] linesBeforeDiff, DiffLine[] diffLines, string[] linesAfterDiff)
        {
            this.FilePaths = new DiffFilePaths(firstFilePath);
            this.FirstFilePath = firstFilePath;
            this.OldStartLine = oldStartLine;
            this.OldLength = oldLength;
            this.NewStartLine = newStartLine;
            this.NewLength = newLength;
            this.LinesBeforeDiff = linesBeforeDiff;
            this.DiffLines = diffLines;
            this.LinesAfterDiff = linesAfterDiff;
        }

        public List<string> GetFilePaths()
        {
            return this.FilePaths.FilePaths.Order().ToList();
        }

        /*
        public string GetId()
        {
            var diffText = new StringBuilder();
            foreach (var line in this.DiffLines)
            {
                diffText.Append(line.GetDiffTypeChar());
                diffText.AppendLine(line.Line);
            }

            var diffBytes = Encoding.UTF8.GetBytes(diffText.ToString());
            var diffHash = SHA256.HashData(diffBytes);

            return Convert.ToHexString(diffHash);
        }
        */

        public bool MatchesDiffLines(DiffLine[] otherDiffLines)
        {
            if (this.DiffLines.Length != otherDiffLines.Length)
            {
                return false;
            }

            for (var i = 0; i < this.DiffLines.Length; ++i)
            {
                if (!this.DiffLines[i].Equals(otherDiffLines[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public void AddFilePath(string filePath)
        {
            this.FilePaths.Add(filePath);
        }
    }
}
