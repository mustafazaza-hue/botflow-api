using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BotFlow.Application.Common.DTOs.Analytics;
using BotFlow.Application.Interfaces;
using BotFlow.Domain.Entities;
using BotFlow.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFlow.Application.Services
{
    public interface IAnalyticsService
    {
        Task<AnalyticsMetricsResponse> GetMetricsAsync(AnalyticsFilterRequest filter);
        Task<IEnumerable<BotPerformanceResponse>> GetTopBotsAsync(AnalyticsFilterRequest filter);
        Task<IEnumerable<PagePerformanceResponse>> GetTopPagesAsync(AnalyticsFilterRequest filter);
        Task<ChartDataResponse> GetMessagesChartAsync(AnalyticsFilterRequest filter);
        Task<ChartDataResponse> GetEngagementChartAsync(AnalyticsFilterRequest filter);
        Task<ChartDataResponse> GetResponseTimeChartAsync(AnalyticsFilterRequest filter);
        Task<ChartDataResponse> GetConversionChartAsync(AnalyticsFilterRequest filter);
        Task<TimeSeriesDataResponse> GetTimeSeriesDataAsync(AnalyticsFilterRequest filter);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            ApplicationDbContext context,
            ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AnalyticsMetricsResponse> GetMetricsAsync(AnalyticsFilterRequest filter)
        {
            try
            {
                // الحل: جلب البيانات أولاً ثم التجميع في الذاكرة
                var analyticsData = await _context.AnalyticsData
                    .Where(a => a.Date >= filter.StartDate.Date && a.Date <= filter.EndDate.Date)
                    .ToListAsync();

                if (filter.BotId.HasValue)
                    analyticsData = analyticsData.Where(a => a.BotId == filter.BotId).ToList();
                
                if (filter.PageId.HasValue)
                    analyticsData = analyticsData.Where(a => a.PageId == filter.PageId).ToList();
                
                if (!string.IsNullOrEmpty(filter.Platform))
                    analyticsData = analyticsData.Where(a => a.Platform == filter.Platform).ToList();

                var metrics = analyticsData
                    .GroupBy(a => a.MetricType)
                    .Select(g => new
                    {
                        MetricType = g.Key,
                        Value = g.Average(a => (double)a.Value) // التجميع في الذاكرة
                    })
                    .ToList();

                return new AnalyticsMetricsResponse
                {
                    Id = Guid.NewGuid(),
                    TotalMessages = (decimal)(metrics.FirstOrDefault(m => m.MetricType == "messages")?.Value ?? 0),
                    EngagementRate = (decimal)(metrics.FirstOrDefault(m => m.MetricType == "engagement")?.Value ?? 0),
                    AvgResponseTime = (decimal)(metrics.FirstOrDefault(m => m.MetricType == "response")?.Value ?? 0),
                    ConversionRate = (decimal)(metrics.FirstOrDefault(m => m.MetricType == "conversion")?.Value ?? 0),
                    Date = DateTime.UtcNow,
                    Period = filter.Period
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics metrics");
                throw;
            }
        }

        public async Task<IEnumerable<BotPerformanceResponse>> GetTopBotsAsync(AnalyticsFilterRequest filter)
        {
            try
            {
                // الحل: استخدام Select بدلاً من Include للحد من client projection
                var bots = await _context.Bots
                    .Where(b => b.IsActive)
                    .OrderByDescending(b => b.TotalConversations)
                    .Take(4)
                    .Select(b => new BotPerformanceResponse
                    {
                        BotId = b.Id,
                        BotName = b.Name,
                        Conversations = b.TotalConversations,
                        ConversionRate = b.ConversionRate,
                        IconColor = GetBotIconColor(b.Id)
                    })
                    .ToListAsync();

                return bots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top bots");
                throw;
            }
        }

        public async Task<IEnumerable<PagePerformanceResponse>> GetTopPagesAsync(AnalyticsFilterRequest filter)
        {
            try
            {
                // الحل: استخدام Select بدلاً من Include
                var pages = await _context.Pages
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.MessagesCount)
                    .Take(4)
                    .Select(p => new PagePerformanceResponse
                    {
                        PageId = p.Id,
                        PageName = p.Name,
                        Messages = p.MessagesCount,
                        EngagementRate = p.EngagementRate,
                        Platform = p.Platform,
                        IconColor = GetPageIconColor(p.Platform)
                    })
                    .ToListAsync();

                return pages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top pages");
                throw;
            }
        }

        public async Task<ChartDataResponse> GetMessagesChartAsync(AnalyticsFilterRequest filter)
        {
            try
            {
                // الحل: التجميع في الذاكرة
                var data = await _context.AnalyticsData
                    .Where(a => a.MetricType == "messages" && 
                               a.Date >= filter.StartDate.Date && a.Date <= filter.EndDate.Date)
                    .ToListAsync();

                var groupedData = data
                    .GroupBy(a => a.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Value = g.Sum(a => (double)a.Value) // التجميع في الذاكرة
                    })
                    .ToList();

                return new ChartDataResponse
                {
                    Labels = groupedData.Select(d => d.Date.ToString("ddd")).ToArray(),
                    Values = groupedData.Select(d => (decimal)d.Value).ToArray(),
                    ChartType = "line",
                    Color = "#6366F1"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages chart data");
                throw;
            }
        }

        public async Task<ChartDataResponse> GetEngagementChartAsync(AnalyticsFilterRequest filter)
        {
            try
            {
                // الحل: التجميع في الذاكرة
                var data = await _context.AnalyticsData
                    .Where(a => a.MetricType == "engagement" && 
                               a.Date >= filter.StartDate.Date && a.Date <= filter.EndDate.Date)
                    .ToListAsync();

                var groupedData = data
                    .GroupBy(a => a.Platform)
                    .Select(g => new
                    {
                        Platform = g.Key,
                        Value = g.Average(a => (double)a.Value) // التجميع في الذاكرة
                    })
                    .ToList();

                return new ChartDataResponse
                {
                    Labels = groupedData.Select(d => d.Platform).ToArray(),
                    Values = groupedData.Select(d => (decimal)d.Value).ToArray(),
                    ChartType = "pie",
                    Color = "#1877F2"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting engagement chart data");
                throw;
            }
        }

        public async Task<ChartDataResponse> GetResponseTimeChartAsync(AnalyticsFilterRequest filter)
        {
            try
            {
                // هذه بيانات تجريبية، لذا لا يوجد مشكلة
                var ranges = new[] { "<1min", "1-5min", "5-15min", "15-30min", ">30min" };
                var values = new decimal[] { 45, 30, 15, 7, 3 };

                return new ChartDataResponse
                {
                    Labels = ranges,
                    Values = values,
                    ChartType = "bar",
                    Color = "#EC4899"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting response time chart data");
                throw;
            }
        }

        public async Task<ChartDataResponse> GetConversionChartAsync(AnalyticsFilterRequest filter)
        {
            try
            {
                // هذه بيانات تجريبية، لذا لا يوجد مشكلة
                var stages = new[] { "Visitors", "Engaged", "Qualified", "Converted" };
                var values = new decimal[] { 10000, 7840, 5230, 3420 };

                return new ChartDataResponse
                {
                    Labels = stages,
                    Values = values,
                    ChartType = "funnel",
                    Color = "#8B5CF6"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversion chart data");
                throw;
            }
        }

        public async Task<TimeSeriesDataResponse> GetTimeSeriesDataAsync(AnalyticsFilterRequest filter)
        {
            try
            {
                // الحل: التجميع في الذاكرة
                var data = await _context.AnalyticsData
                    .Where(a => a.Date >= filter.StartDate.Date && a.Date <= filter.EndDate.Date)
                    .ToListAsync();

                var groupedData = data
                    .GroupBy(a => a.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalValue = g.Sum(a => (double)a.Value) // التجميع في الذاكرة
                    })
                    .ToList();

                return new TimeSeriesDataResponse
                {
                    Dates = groupedData.Select(d => d.Date).ToArray(),
                    Values = groupedData.Select(d => (decimal)d.TotalValue).ToArray(),
                    MetricName = "Total Activity"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting time series data");
                throw;
            }
        }

        private string GetBotIconColor(Guid botId)
        {
            // Generate consistent color based on bot ID
            var colors = new[]
            {
                "from-[#6366F1] to-[#8B5CF6]",
                "from-[#8B5CF6] to-[#EC4899]",
                "from-[#EC4899] to-[#6366F1]",
                "from-blue-500 to-blue-600"
            };
            
            var index = Math.Abs(botId.GetHashCode()) % colors.Length;
            return colors[index];
        }

        private string GetPageIconColor(string platform)
        {
            return platform.ToLower() switch
            {
                "facebook" => "bg-blue-500",
                "instagram" => "bg-gradient-to-br from-purple-500 to-pink-500",
                _ => "bg-gray-500"
            };
        }
    }
}