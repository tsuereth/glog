using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.Model;
using DiffPlex.Renderer;

namespace GlogGenerator.DiffSummary
{
    public class DiffSummaryTracker
    {
        private List<DiffPattern> patterns = new List<DiffPattern>();

        private List<string> changedBinaryPaths = new List<string>();
        private List<string> onlyInCompareToPaths = new List<string>();
        private List<string> onlyInCurrentPaths = new List<string>();

        private readonly IDiffer differ;
        private readonly IChunker diffChunker;

        public DiffSummaryTracker()
        {
            this.differ = new Differ();
            this.diffChunker = new LineChunker();
        }

        public void AddTextFile(string filePath, string compareToText, string currentText)
        {
            var diffResult = this.differ.CreateDiffs(compareToText, currentText, ignoreWhiteSpace: false, ignoreCase: false, this.diffChunker);

            // DiffPlex's UnidiffRenderer `CreateHunks()` does EXACTLY what we want,
            // collecting diff lines and line numbers into "hunks" of modified text.
            // But the class method is private. :(
            //
            // As of writing, the method signature is:
            //   private List<DiffHunk> CreateHunks(DiffResult diffResult)
            var unidiffRenderer = new UnidiffRenderer(this.differ, contextLines: 3);
            var createHunksMethod = unidiffRenderer.GetType().GetMethod("CreateHunks", BindingFlags.NonPublic | BindingFlags.Instance);
            var diffHunksObject = createHunksMethod.Invoke(unidiffRenderer, new object[] { diffResult });

            // The returned type `DiffHunk` is also private. :(
            var diffHunksCountProperty = diffHunksObject.GetType().GetProperty("Count");
            var diffHunksCount = (int)diffHunksCountProperty.GetValue(diffHunksObject);

            var diffHunksItemProperty = diffHunksObject.GetType().GetProperty("Item");
            for (int i = 0; i < diffHunksCount; ++i)
            {
                var diffHunkObject = diffHunksItemProperty.GetValue(diffHunksObject, new object[] { i });

                var oldStartLineProperty = diffHunkObject.GetType().GetProperty("OldStartLine");
                var oldStartLine = (int)oldStartLineProperty.GetValue(diffHunkObject);
                var oldLengthProperty = diffHunkObject.GetType().GetProperty("OldLength");
                var oldLength = (int)oldLengthProperty.GetValue(diffHunkObject);
                var newStartLineProperty = diffHunkObject.GetType().GetProperty("NewStartLine");
                var newStartLine = (int)newStartLineProperty.GetValue(diffHunkObject);
                var newLengthProperty = diffHunkObject.GetType().GetProperty("NewLength");
                var newLength = (int)newLengthProperty.GetValue(diffHunkObject);

                var linesProperty = diffHunkObject.GetType().GetProperty("Lines");
                var linesObject = linesProperty.GetValue(diffHunkObject);
                var linesCountProperty = linesObject.GetType().GetProperty("Count");
                var linesCount = (int)linesCountProperty.GetValue(linesObject);

                // Convert the private DiffLine type into our own, uh, DiffLine type.
                var hunkLines = new List<DiffLine>();
                var linesItemProperty = linesObject.GetType().GetProperty("Item");
                for (int l = 0; l < linesCount; ++l)
                {
                    var lineObject = linesItemProperty.GetValue(linesObject, new object[] { l });

                    var lineTypeProperty = lineObject.GetType().GetProperty("Type");
                    var lineTypeObject = lineTypeProperty.GetValue(lineObject);
                    var lineTypeEnumName = lineTypeObject.GetType().GetEnumName(lineTypeObject);

                    var lineTextProperty = lineObject.GetType().GetProperty("Text");
                    var lineText = (string)lineTextProperty.GetValue(lineObject);

                    switch (lineTypeEnumName)
                    {
                        case "Unchanged":
                            hunkLines.Add(new DiffLine(DiffLine.DiffLineType.Unchanged, lineText));
                            break;
                        case "Deleted":
                            hunkLines.Add(new DiffLine(DiffLine.DiffLineType.Removed, lineText));
                            break;
                        case "Inserted":
                            hunkLines.Add(new DiffLine(DiffLine.DiffLineType.Added, lineText));
                            break;
                        default:
                            throw new NotImplementedException($"Unrecognized DiffPlex line type {lineTypeEnumName}");
                    }
                }

                // Isolate pre-diff context lines, diff lines, and post-diff context lines.

                var linesBeforeDiffCount = 0;
                while (hunkLines[linesBeforeDiffCount].DiffType == DiffLine.DiffLineType.Unchanged)
                {
                    ++linesBeforeDiffCount;
                }
                var linesBeforeDiff = new string[0];
                if (linesBeforeDiffCount > 0)
                {
                    linesBeforeDiff = hunkLines.GetRange(0, linesBeforeDiffCount).Select(l => l.Line).ToArray();
                }

                var linesAfterDiffCount = 0;
                while (hunkLines[hunkLines.Count - (linesAfterDiffCount + 1)].DiffType == DiffLine.DiffLineType.Unchanged)
                {
                    ++linesAfterDiffCount;
                }
                var linesAfterDiff = new string[0];
                if (linesAfterDiffCount > 0)
                {
                    linesAfterDiff = hunkLines.GetRange(hunkLines.Count - linesAfterDiffCount, linesAfterDiffCount).Select(l => l.Line).ToArray();
                }

                var diffLinesCount = hunkLines.Count - (linesBeforeDiffCount + linesAfterDiffCount);
                var diffLines = hunkLines.GetRange(linesBeforeDiffCount, diffLinesCount).ToArray();

                var foundMatchingPattern = false;
                for (int p = 0; p < this.patterns.Count; ++p)
                {
                    if (this.patterns[p].MatchesDiffLines(diffLines))
                    {
                        this.patterns[p].AddFilePath(filePath);
                        foundMatchingPattern = true;
                        break;
                    }
                }

                if (!foundMatchingPattern)
                {
                    var pattern = new DiffPattern(filePath, oldStartLine, oldLength, newStartLine, newLength, linesBeforeDiff, diffLines, linesAfterDiff);

                    this.patterns.Add(pattern);
                }
            }
        }

        public void AddBinaryFile(string filePath, byte[] compareToBytes, byte[] currentBytes)
        {
            if (!compareToBytes.SequenceEqual(currentBytes))
            {
                this.changedBinaryPaths.Add(filePath);
            }
        }

        public void AddFileOnlyInCompareTo(string filePath)
        {
            this.onlyInCompareToPaths.Add(filePath);
        }

        public void AddFileOnlyInCurrent(string filePath)
        {
            this.onlyInCurrentPaths.Add(filePath);
        }

        public void WriteSummary(Stream outputStream)
        {
            // Assemble diff patterns into sections by common filepaths.
            var patternSections = new Dictionary<string, TextFileDiffPatternsSection>();
            foreach (var pattern in this.patterns)
            {
                var sectionFilePaths = string.Join(':', pattern.GetFilePaths());
                if (patternSections.TryGetValue(sectionFilePaths, out var matchingPattern))
                {
                    matchingPattern.AddPattern(pattern);
                }
                else
                {
                    patternSections.Add(sectionFilePaths, new TextFileDiffPatternsSection(pattern));
                }
            }

            // List all tracked diff information into sortable "sections" before writing them.
            var diffSections = new List<IDiffSummarySection>();
            diffSections.AddRange(patternSections.Values);
            foreach (var changedBinaryPath in this.changedBinaryPaths)
            {
                diffSections.Add(new BinaryFileDiffSection(changedBinaryPath));
            }
            foreach (var onlyInCompareToPath in this.onlyInCompareToPaths)
            {
                diffSections.Add(new OnlyInCompareToDiffSection(onlyInCompareToPath));
            }
            foreach (var onlyInCurrentPath in this.onlyInCurrentPaths)
            {
                diffSections.Add(new OnlyInCurrentDiffSection(onlyInCurrentPath));
            }

            if (diffSections.Count == 0)
            {
                using (var writer = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true))
                {
                    writer.WriteLine("No differences found.");
                }
                return;
            }

            foreach (var diffSection in diffSections.OrderBy(s => s.GetSectionFilePaths()))
            {
                diffSection.WriteSection(outputStream);
            }
        }
    }
}
