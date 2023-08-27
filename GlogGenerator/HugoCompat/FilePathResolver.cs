using System;
using System.Collections.Generic;
using System.IO;

namespace GlogGenerator.HugoCompat
{
    public class FilePathResolver
    {
        public string BasePath { get; private set; }

        private List<string> searchPaths = new List<string>();

        public FilePathResolver()
        {
            this.BasePath = Directory.GetCurrentDirectory();
        }

        public FilePathResolver(string basePath)
        {
            this.BasePath = basePath;
        }

        public void AddSearchPath(string addPath)
        {
            this.searchPaths.Add(addPath);
        }

        public string Resolve(string referenceBasePath, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            // Special case: if filePath "looks" absolute, resolve it against the shared basePath.
            // ("Looks absolute" means it starts with a forward slash, regardless of the current platform.)
            if (filePath[0] == '/')
            {
                filePath = filePath.Remove(0, 1);

                var pathCheck = Path.Combine(this.BasePath, filePath);

                if (File.Exists(pathCheck))
                {
                    return pathCheck;
                }

                throw new FileNotFoundException($"Couldn't find \"{filePath}\" in base path \"{this.BasePath}\"");
            }

            var resolveSearchPaths = new List<string>(this.searchPaths.Count + 1);
            if (referenceBasePath != null)
            {
                resolveSearchPaths.Add(referenceBasePath);
            }
            resolveSearchPaths.AddRange(this.searchPaths);

            foreach (var searchPath in resolveSearchPaths)
            {
                var pathCheck = Path.Combine(searchPath, filePath);

                if (File.Exists(pathCheck))
                {
                    return pathCheck;
                }
            }

            throw new FileNotFoundException($"Couldn't find \"{filePath}\" in any search path: {string.Join(';', resolveSearchPaths)}");
        }

        public string Resolve(string filePath)
        {
            return this.Resolve(null, filePath);
        }
    }
}
