using System;
using System.IO;

namespace GlogGenerator
{
    public static class PathExtensions
    {
        public static string[] GetPathPartsWithStartingDirName(this string path, string startingDirName)
        {
            var pathParts = path.Split(Path.DirectorySeparatorChar);
            var startingDirIndex = Array.FindIndex(pathParts, p => p.Equals(startingDirName, StringComparison.Ordinal));
            if (startingDirIndex < 0)
            {
                throw new ArgumentException($"File path {path} doesn't appear to contain starting directory {startingDirName}");
            }

            return pathParts[startingDirIndex..];
        }
    }
}
