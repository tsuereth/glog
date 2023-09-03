using System;
using System.IO;
using System.Linq;

namespace GlogGenerator.Data
{
    public class StaticFileData
    {
        public static readonly string StaticContentBaseDir = "static";

        public string SourceFilePath { get; private set; } = string.Empty;

        public string FileName { get; private set; } = string.Empty;

        public string OutputDirRelative { get; private set; } = string.Empty;

        public static StaticFileData FromFilePath(string filePath)
        {
            var sourcePathParts = filePath.Split(Path.DirectorySeparatorChar);
            var baseDirIndex = Array.FindIndex(sourcePathParts, p => p.Equals(StaticContentBaseDir, StringComparison.OrdinalIgnoreCase));
            if (baseDirIndex < 0)
            {
                throw new ArgumentException($"Static file path {filePath} doesn't appear to contain base directory {StaticContentBaseDir}");
            }

            var file = new StaticFileData();
            file.SourceFilePath = filePath;

            var outputDirParts = sourcePathParts[(baseDirIndex + 1)..^1];
            file.OutputDirRelative = string.Join('/', outputDirParts);
            file.FileName = sourcePathParts.Last();

            return file;
        }
    }
}
