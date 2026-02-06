using BotFlow.Application.Common.DTOs;
using BotFlow.Application.Common.DTOs.Users;

namespace BotFlow.Application.Interfaces
{
    public interface IKPIService
    {
        // ==================== Dashboard Methods ====================
        Task<Common.DTOs.DashboardOverviewDto> GetDashboardOverviewAsync();
        Task<List<Common.DTOs.RevenueTrendDto>> GetRevenueTrendAsync(string period = "monthly");
        Task<Common.DTOs.SubscriptionDistributionDto> GetSubscriptionDistributionAsync();
        Task<List<Common.DTOs.RecentUserDto>> GetRecentUsersAsync(int page = 1, int pageSize = 10);
        Task<Common.DTOs.SystemPerformanceDto> GetSystemPerformanceAsync();
        Task<Common.DTOs.UserStatsDto> GetUserStatsAsync();

        // ==================== KPI Analytics Methods ====================
        Task<Common.DTOs.KPIOverviewDto> GetKPIOverviewAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<Common.DTOs.UserGrowthDto>> GetUserGrowthTrendAsync(string period = "monthly");
        Task<List<Common.DTOs.ApiUsageDto>> GetApiUsageAsync(DateTime startDate, DateTime endDate);
        Task<Common.DTOs.SystemHealthDto> GetSystemHealthAsync();
        Task<Common.DTOs.CostBreakdownDto> GetCostBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<Common.DTOs.MessageVolumeDto>> GetMessageVolumeAsync(DateTime startDate, DateTime endDate);

        // ==================== Additional KPI Methods ====================
        Task<List<KPIMetricDto>> GetKPIMetricsAsync(string metricType = "", string period = "daily", 
            DateTime? startDate = null, DateTime? endDate = null);
        Task<List<RevenueAnalysisDto>> GetRevenueAnalysisAsync(DateTime startDate, DateTime endDate);
        Task UpdateSystemStatisticsAsync();
        Task<SystemStatisticDto> GetLatestSystemStatisticsAsync();

        // ==================== Helper Methods ====================
        Task<List<object>> GetKPIMetricsChartDataAsync(string metricType, string period);
        Task<decimal> CalculateRevenueGrowthAsync(DateTime startDate, DateTime endDate);
        Task<decimal> CalculateUserGrowthAsync(DateTime startDate, DateTime endDate);
    }

    // ==================== DTOs for KPIService ====================
    public class KPIMetricDto
    {
        public Guid Id { get; set; }
        public string MetricType { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public decimal? TargetValue { get; set; }
        public double? ChangePercentage { get; set; }
        public string? Platform { get; set; }
        public string? Category { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RevenueAnalysisDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int NewUsers { get; set; }
        public int ActiveUsers { get; set; }
        public decimal AverageRevenuePerUser { get; set; }
        public decimal ChurnRate { get; set; }
        public decimal LifetimeValue { get; set; }
    }

    public class SystemStatisticDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TrialUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public int TotalBots { get; set; }
        public int TotalDocuments { get; set; }
        public int ActiveDataSources { get; set; }
        public int ProcessingDataSources { get; set; }
        public int FailedDataSources { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal UXPilotApiCost { get; set; }
        public decimal WhatsAppApiCost { get; set; }
        public int AvgResponseTime { get; set; }
        public decimal ServerUptime { get; set; }
        public decimal DatabaseLoad { get; set; }
        public decimal BotSuccessRate { get; set; }
        public decimal ErrorRate { get; set; }
        public int TotalComments { get; set; }
        public int MessagesSent { get; set; }
        public int UXPilotApiCalls { get; set; }
        public int WhatsAppApiCalls { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}