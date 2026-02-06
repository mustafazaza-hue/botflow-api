namespace BotFlow.Application.Common.DTOs.AIDataSources
{
    public class DataSourceStatsDto
    {
        public int TotalDocuments { get; set; }
        public int ActiveSources { get; set; }
        public int ProcessingSources { get; set; }
        public int FailedSources { get; set; }
        public int TotalSources { get; set; }
    }
}