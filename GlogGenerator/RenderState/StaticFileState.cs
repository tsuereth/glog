using System;
using System.IO;
using System.Net;
using GlogGenerator.Data;
using Microsoft.AspNetCore.StaticFiles;

namespace GlogGenerator.RenderState
{
    public class StaticFileState : IOutputContent
    {
        public StaticFileData StaticFile { get; set; }

        public string OutputPathRelative { get; set; } = string.Empty;

        public static StaticFileState FromStaticFileData(StaticFileData fileData)
        {
            var file = new StaticFileState();
            file.StaticFile = fileData;

            if (string.IsNullOrEmpty(fileData.OutputDirRelative))
            {
                file.OutputPathRelative = fileData.FileName;
            }
            else
            {
                file.OutputPathRelative = $"{fileData.OutputDirRelative}/{fileData.FileName}";
            }

            return file;
        }

        public void WriteFile(SiteState site, string filePath)
        {
            var outputDir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(outputDir))
            {
                throw new ArgumentException($"Static file output path {filePath} has empty dirname");
            }

            var canonicalOutputDirPath = outputDir + Path.DirectorySeparatorChar;
            if (!Directory.Exists(canonicalOutputDirPath))
            {
                Directory.CreateDirectory(canonicalOutputDirPath);
            }

            File.Copy(this.StaticFile.SourceFilePath, filePath, overwrite: true);
        }

        public void WriteHttpListenerResponse(SiteState site, ref HttpListenerResponse response)
        {
            var typeProvider = new FileExtensionContentTypeProvider();
            if (typeProvider.TryGetContentType(this.StaticFile.SourceFilePath, out var contentType))
            {
                response.ContentType = contentType;
            }
            else
            {
                response.ContentType = "application/octet-stream";
            }

            using (var fileStream = File.OpenRead(this.StaticFile.SourceFilePath))
            {
                response.ContentLength64 = fileStream.Length;

                var fileReadBuffer = new byte[4 * 1024 * 1024];
                using (var byteWriter = new BinaryWriter(response.OutputStream))
                {
                    var fileReadBytes = 0;
                    while ((fileReadBytes = fileStream.Read(fileReadBuffer, 0, fileReadBuffer.Length)) > 0)
                    {
                        byteWriter.Write(fileReadBuffer, 0, fileReadBytes);
                    }

                    byteWriter.Close();
                }
            }
        }
    }
}
