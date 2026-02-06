namespace BotFlow.Application.Common.DTOs.SuperAdmin
{
    public class DashboardOverviewDto
    {
        public int TotalUsers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TrialUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int ActiveBots { get; set; }
        public int TotalBots { get; set; }
        public int TotalDocuments { get; set; }
        public int ActiveDataSources { get; set; }
        public int ProcessingDataSources { get; set; }
        public int FailedDataSources { get; set; }
        public decimal UserGrowthPercentage { get; set; }
        public decimal RevenueGrowthPercentage { get; set; }
        public decimal BotGrowthPercentage { get; set; }
    }
}