using System.IO;
using System.Text;

namespace GlogGenerator.DiffSummary
{
    public class OnlyInCurrentDiffSection : IDiffSummarySection
    {
        private string filePath;

        public OnlyInCurrentDiffSection(string filePath)
        {
            this.filePath = filePath;
        }

        public string GetSectionFilePaths()
        {
            return this.filePath;
        }

        public void WriteSection(Stream outputStream)
        {
            using (var writer = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true))
            {
                writer.WriteLine($"Only in current: {this.filePath}");
            }
        }
    }
}
