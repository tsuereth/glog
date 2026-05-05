using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GlogGenerator.DiffSummary
{
    public class DiffFilePaths
    {
        public List<string> FilePaths { get; private set; } = new List<string>();

        public DiffFilePaths(string filePath)
        {
            this.Add(filePath);
        }

        public void Add(string filePath)
        {
            // Always store paths internally with unix-style path separators '/'
            var normalizedFilePath = filePath.Replace(Path.DirectorySeparatorChar, '/');
            this.FilePaths.Add(filePath);
        }

        public int Count()
        {
            return this.FilePaths.Count;
        }

        public List<string> GetBasePaths()
        {
            var basePaths = new HashSet<string>();
            foreach (var filePath in this.FilePaths)
            {
                var directoryPos = filePath.IndexOf('/');
                if (directoryPos == -1)
                {
                    basePaths.Add(filePath);
                }
                else
                {
                    basePaths.Add(filePath.Substring(0, directoryPos + 1));
                }
            }

            return basePaths.Order().ToList();
        }
    }
}
