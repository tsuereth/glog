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
            var relativePathParts = filePath.GetPathPartsWithStartingDirName(StaticContentBaseDir);

            var file = new StaticFileData();
            file.SourceFilePath = filePath;
            file.OutputDirRelative = string.Join('/', relativePathParts[1..^1]);
            file.FileName = relativePathParts.Last();

            return file;
        }
    }
}
