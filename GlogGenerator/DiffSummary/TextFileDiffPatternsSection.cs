using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GlogGenerator.DiffSummary
{
    public class TextFileDiffPatternsSection : IDiffSummarySection
    {
        public const int MaxFilePathsCount = 10;

        private List<DiffPattern> patterns = new List<DiffPattern>();

        public TextFileDiffPatternsSection(DiffPattern pattern)
        {
            this.patterns.Add(pattern);
        }

        public bool MatchesFilePaths(DiffPattern pattern)
        {
            var filePaths = this.patterns[0].GetFilePaths();
            var otherFilePaths = pattern.GetFilePaths();
            return filePaths.SequenceEqual(otherFilePaths);
        }

        public void AddPattern(DiffPattern pattern)
        {
            if (!this.MatchesFilePaths(pattern))
            {
                throw new ArgumentException($"Attempted to add a pattern with different filepaths");
            }

            this.patterns.Add(pattern);
        }

        public string GetSectionFilePaths()
        {
            // Every pattern has the same FilePaths.
            return string.Join(':', this.patterns[0].GetFilePaths());
        }

        public void WriteSection(Stream outputStream)
        {
            using (var writer = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true))
            {
                var pathsCount = this.patterns[0].FilePaths.Count();

                if (pathsCount == 1)
                {
                    writer.WriteLine($"--- {this.patterns[0].FirstFilePath}");
                    writer.WriteLine($"+++ {this.patterns[0].FirstFilePath}");
                }
                else
                {
                    var basePaths = this.patterns[0].FilePaths.GetBasePaths();
                    if (basePaths.Count < MaxFilePathsCount)
                    {
                        writer.WriteLine($"--- {pathsCount} files in {string.Join(", ", basePaths)}");
                    }
                    else
                    {
                        writer.WriteLine($"--- {pathsCount} files in .");
                    }

                    writer.WriteLine($"+++ (example) {this.patterns[0].FirstFilePath}");
                }

                foreach (var pattern in this.patterns.OrderBy(p => p.OldStartLine))
                {
                    writer.WriteLine($"@@ -{pattern.OldStartLine},{pattern.OldLength} +{pattern.NewStartLine},{pattern.NewLength} @@");

                    for (int l = 0; l < pattern.LinesBeforeDiff.Length; ++l)
                    {
                        writer.WriteLine(pattern.LinesBeforeDiff[l]);
                    }

                    for (int l = 0; l < pattern.DiffLines.Length; ++l)
                    {
                        writer.WriteLine(pattern.DiffLines[l].ToString());
                    }

                    for (int l = 0; l < pattern.LinesAfterDiff.Length; ++l)
                    {
                        writer.WriteLine(pattern.LinesAfterDiff[l]);
                    }
                }
            }
        }
    }
}
