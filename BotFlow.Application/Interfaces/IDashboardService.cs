using BotFlow.Application.Common.DTOs;

namespace BotFlow.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardMetricsDto> GetDashboardMetricsAsync(Guid userId);
        Task<ConversationTrendDto> GetConversationTrendAsync(Guid userId, string timeRange = "weekly");
        Task<ResponseTimeDto> GetResponseTimesAsync(Guid userId);
        Task<EngagementSourcesDto> GetEngagementSourcesAsync(Guid userId);
        Task<List<RecentActivityDto>> GetRecentActivitiesAsync(Guid userId, int count = 10);
        Task<DashboardAlertDto> GetAlertsAsync(Guid userId);
        Task<DashboardExportDto> ExportDashboardDataAsync(Guid userId, TimeRangeDto timeRange);
    }
}