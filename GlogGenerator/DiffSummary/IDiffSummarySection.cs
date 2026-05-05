using System.IO;

namespace GlogGenerator.DiffSummary
{
    public interface IDiffSummarySection
    {
        public string GetSectionFilePaths();

        public void WriteSection(Stream outputStream);
    }
}
