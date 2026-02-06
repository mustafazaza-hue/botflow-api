namespace BotFlow.Application.Common.DTOs
{
    public class DashboardMetricsDto
    {
        public int ActivePages { get; set; }
        public int ConversationsToday { get; set; }
        public double ResponseRate { get; set; }
        public string BotsActive { get; set; } = string.Empty;
        
        public List<MetricChangeDto> KeyMetrics { get; set; } = new();
        public List<DashboardBotPerformanceDto> BotPerformances { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        
        public ConversationTrendDto ConversationTrend { get; set; } = new();
        public ResponseTimeDto ResponseTimes { get; set; } = new();
        public EngagementSourcesDto EngagementSources { get; set; } = new();
    }
    
    public class MetricChangeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Change { get; set; } = string.Empty;
        public string ChangeColor { get; set; } = "text-green-600";
        public string BgColor { get; set; } = "from-blue-500 to-blue-600";
        public string Icon { get; set; } = string.Empty;
    }
    
    // تغيير الاسم لتجنب التكرار
    public class DashboardBotPerformanceDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Conversations { get; set; }
        public string? SuccessRate { get; set; }
        public string? Status { get; set; }
        public string BgColor { get; set; } = "from-blue-500 to-blue-600";
    }
    
    public class RecentActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BgColor { get; set; } = "from-blue-500 to-blue-600";
        public string Icon { get; set; } = string.Empty;
    }
    
    public class ConversationTrendDto
    {
        public List<string> Days { get; set; } = new();
        public List<int> ConversationCounts { get; set; } = new();
    }
    
    public class ResponseTimeDto
    {
        public List<string> TimeSlots { get; set; } = new();
        public List<double> ResponseTimes { get; set; } = new();
        public List<string> Colors { get; set; } = new();
    }
    
    public class EngagementSourcesDto
    {
        public List<string> Platforms { get; set; } = new();
        public List<int> Percentages { get; set; } = new();
        public List<string> Colors { get; set; } = new();
    }
    
    public class TimeRangeDto
    {
        public string Range { get; set; } = "weekly";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    
    public class DashboardAlertDto
    {
        public bool HasAlerts { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "warning";
        public DateTime Timestamp { get; set; }
    }
    
    public class DashboardExportDto
    {
        public string Message { get; set; } = string.Empty;
        public Guid ExportId { get; set; }
        public DateTime EstimatedCompletion { get; set; }
        public string Format { get; set; } = "PDF";
        public string DownloadUrl { get; set; } = string.Empty;
    }
}