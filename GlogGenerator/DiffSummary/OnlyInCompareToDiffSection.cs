using System.IO;
using System.Text;

namespace GlogGenerator.DiffSummary
{
    public class OnlyInCompareToDiffSection : IDiffSummarySection
    {
        private string filePath;

        public OnlyInCompareToDiffSection(string filePath)
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
                writer.WriteLine($"Only in compare-to: {this.filePath}");
            }
        }
    }
}
