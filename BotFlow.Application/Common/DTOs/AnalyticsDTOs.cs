using System;

namespace BotFlow.Application.Common.DTOs.Analytics
{
    // DTOs for Analytics Page
    public class AnalyticsMetricsResponse
    {
        public Guid Id { get; set; }
        public decimal TotalMessages { get; set; }
        public decimal EngagementRate { get; set; }
        public decimal AvgResponseTime { get; set; }
        public decimal ConversionRate { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; } = string.Empty;
    }

    public class BotPerformanceResponse
    {
        public Guid BotId { get; set; }
        public string BotName { get; set; } = string.Empty;
        public int Conversations { get; set; }
        public decimal ConversionRate { get; set; }
        public string IconColor { get; set; } = string.Empty;
    }

    public class PagePerformanceResponse
    {
        public Guid PageId { get; set; }
        public string PageName { get; set; } = string.Empty;
        public int Messages { get; set; }
        public decimal EngagementRate { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
    }

    public class ChartDataResponse
    {
        public string[] Labels { get; set; } = Array.Empty<string>();
        public decimal[] Values { get; set; } = Array.Empty<decimal>();
        public string ChartType { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class TimeSeriesDataResponse
    {
        public DateTime[] Dates { get; set; } = Array.Empty<DateTime>();
        public decimal[] Values { get; set; } = Array.Empty<decimal>();
        public string MetricName { get; set; } = string.Empty;
    }

    public class AnalyticsFilterRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; } = "daily"; // daily, weekly, monthly
        public Guid? BotId { get; set; }
        public Guid? PageId { get; set; }
        public string Platform { get; set; } = string.Empty;
    }
}