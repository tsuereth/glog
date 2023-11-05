using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlogGenerator.IgdbApi;
using GlogGenerator.Stats;
using Markdig;

namespace GlogGenerator
{
    public interface ISiteBuilder
    {
        public MarkdownPipeline GetMarkdownPipeline();

        public Task UpdateIgdbCacheFromApiAsync(IgdbApiClient apiClient);

        public void UpdateDataIndex();

        public List<GameStats> GetGameStatsForDateRange(DateTimeOffset startDate, DateTimeOffset endDate);

        public void UpdateContentRoutes();
    }
}
