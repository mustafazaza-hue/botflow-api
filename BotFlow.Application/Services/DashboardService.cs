using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BotFlow.Application.Common.DTOs;
using BotFlow.Application.Interfaces;
using BotFlow.Domain.Entities;
using BotFlow.Domain.Enums;
using BotFlow.Infrastructure.Data;

namespace BotFlow.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardMetricsDto> GetDashboardMetricsAsync(Guid userId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);

                var activePages = await _context.SocialPages
                    .CountAsync(p => p.UserId == userId && p.IsActive);

                var conversationsToday = await _context.Conversations
                    .CountAsync(c => c.UserId == userId && c.CreatedAt.Date == today);

                var responseRate = await CalculateResponseRateAsync(userId);
                var botsActive = await GetBotsActiveStatusAsync(userId);

                var keyMetrics = await GetKeyMetricsWithChangesAsync(userId, weekStart, today);
                var botPerformances = await GetBotPerformancesAsync(userId);
                var recentActivities = await GetRecentActivitiesAsync(userId, 4);
                var conversationTrend = await GetConversationTrendAsync(userId, "weekly");
                var responseTimes = await GetResponseTimesAsync(userId);
                var engagementSources = await GetEngagementSourcesAsync(userId);

                return new DashboardMetricsDto
                {
                    ActivePages = activePages,
                    ConversationsToday = conversationsToday,
                    ResponseRate = responseRate,
                    BotsActive = botsActive,
                    KeyMetrics = keyMetrics,
                    BotPerformances = botPerformances,
                    RecentActivities = recentActivities,
                    ConversationTrend = conversationTrend,
                    ResponseTimes = responseTimes,
                    EngagementSources = engagementSources
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard metrics for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ConversationTrendDto> GetConversationTrendAsync(Guid userId, string timeRange = "weekly")
        {
            DateTime startDate;
            List<string> days;

            switch (timeRange.ToLower())
            {
                case "daily":
                    startDate = DateTime.UtcNow.AddDays(-1);
                    days = Enumerable.Range(0, 24).Select(h => $"{h:00}:00").ToList();
                    break;
                case "monthly":
                    startDate = DateTime.UtcNow.AddDays(-30);
                    days = Enumerable.Range(0, 30).Select(d => DateTime.UtcNow.AddDays(-d).ToString("MMM dd")).Reverse().ToList();
                    break;
                default: // weekly
                    startDate = DateTime.UtcNow.AddDays(-7);
                    days = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                    break;
            }

            var trend = new ConversationTrendDto { Days = days };
            trend.ConversationCounts = GetDefaultTrendData(timeRange);

            return trend;
        }

        public async Task<ResponseTimeDto> GetResponseTimesAsync(Guid userId)
        {
            var timeSlots = new List<string> { "00:00", "04:00", "08:00", "12:00", "16:00", "20:00" };
            var colors = new List<string> { "#8B5CF6", "#8B5CF6", "#8B5CF6", "#EC4899", "#8B5CF6", "#8B5CF6" };
            var responseTimes = new List<double> { 2.5, 1.8, 3.2, 4.5, 3.8, 2.9 };

            return new ResponseTimeDto
            {
                TimeSlots = timeSlots,
                ResponseTimes = responseTimes,
                Colors = colors
            };
        }

        public async Task<EngagementSourcesDto> GetEngagementSourcesAsync(Guid userId)
        {
            var platforms = new List<string> { "Facebook", "Instagram", "Direct Messages", "Comments" };
            var percentages = new List<int> { 45, 30, 15, 10 };
            var colors = new List<string> { "#6366F1", "#8B5CF6", "#EC4899", "#F59E0B" };

            return new EngagementSourcesDto
            {
                Platforms = platforms,
                Percentages = percentages,
                Colors = colors
            };
        }

        public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(Guid userId, int count = 10)
        {
            return GetDefaultRecentActivities();
        }

        public async Task<DashboardAlertDto> GetAlertsAsync(Guid userId)
        {
            var hasAlerts = true;

            return new DashboardAlertDto
            {
                HasAlerts = hasAlerts,
                Title = "Bot Status Alert",
                Message = "2 bots are currently inactive. Review and activate them to maintain engagement.",
                Type = "warning",
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<DashboardExportDto> ExportDashboardDataAsync(Guid userId, TimeRangeDto timeRange)
        {
            var exportId = Guid.NewGuid();
            var downloadUrl = $"/api/exports/{exportId}";

            return new DashboardExportDto
            {
                Message = "Export started successfully",
                ExportId = exportId,
                EstimatedCompletion = DateTime.UtcNow.AddSeconds(30),
                Format = "PDF",
                DownloadUrl = downloadUrl
            };
        }

        private async Task<double> CalculateResponseRateAsync(Guid userId)
        {
            return 94.2;
        }

        private async Task<string> GetBotsActiveStatusAsync(Guid userId)
        {
            return "12/14";
        }

        private async Task<List<MetricChangeDto>> GetKeyMetricsWithChangesAsync(Guid userId, DateTime weekStart, DateTime today)
        {
            return new List<MetricChangeDto>
            {
                new()
                {
                    Id = "active-pages",
                    Value = "8",
                    Label = "Active Pages",
                    Change = "+12%",
                    ChangeColor = "text-green-600",
                    BgColor = "from-blue-500 to-blue-600",
                    Icon = "faShareNodes"
                },
                new()
                {
                    Id = "conversations",
                    Value = "247",
                    Label = "Conversations Today",
                    Change = "+28%",
                    ChangeColor = "text-green-600",
                    BgColor = "from-purple-500 to-purple-600",
                    Icon = "faComments"
                },
                new()
                {
                    Id = "response-rate",
                    Value = "94.2%",
                    Label = "Response Rate",
                    Change = "+8%",
                    ChangeColor = "text-green-600",
                    BgColor = "from-green-500 to-green-600",
                    Icon = "faChartLine"
                },
                new()
                {
                    Id = "bots-active",
                    Value = "12/14",
                    Label = "Bots Active",
                    Change = "Active",
                    ChangeColor = "text-green-600",
                    BgColor = "from-pink-500 to-pink-600",
                    Icon = "faRobot"
                }
            };
        }

        private async Task<List<DashboardBotPerformanceDto>> GetBotPerformancesAsync(Guid userId)
        {
            return new List<DashboardBotPerformanceDto>
            {
                new()
                {
                    Id = "ecommerce-bot",
                    Name = "E-commerce Bot",
                    Conversations = 152,
                    SuccessRate = "98%",
                    BgColor = "from-blue-500 to-blue-600"
                },
                new()
                {
                    Id = "support-bot",
                    Name = "Support Bot",
                    Conversations = 89,
                    SuccessRate = "95%",
                    BgColor = "from-purple-500 to-purple-600"
                },
                new()
                {
                    Id = "lead-gen-bot",
                    Name = "Lead Gen Bot",
                    Conversations = 64,
                    SuccessRate = "92%",
                    BgColor = "from-pink-500 to-pink-600"
                },
                new()
                {
                    Id = "faq-bot",
                    Name = "FAQ Bot",
                    Conversations = 0,
                    Status = "Inactive",
                    BgColor = "from-amber-500 to-amber-600"
                }
            };
        }

        private List<int> GetDefaultTrendData(string timeRange)
        {
            return timeRange switch
            {
                "daily" => new List<int> { 12, 8, 15, 20, 18, 22, 25, 30, 28, 32, 35, 30, 25, 28, 32, 35, 40, 38, 35, 30, 25, 20, 15, 10 },
                "monthly" => Enumerable.Range(0, 30).Select(i => new Random().Next(50, 200)).ToList(),
                _ => new List<int> { 180, 220, 195, 247, 230, 210, 240 }
            };
        }

        private List<RecentActivityDto> GetDefaultRecentActivities()
        {
            return new List<RecentActivityDto>
            {
                new()
                {
                    Id = "new-conversation",
                    Title = "New conversation started",
                    Time = "2 min ago",
                    Description = "Customer inquiry about product availability on Instagram",
                    BgColor = "from-blue-500 to-blue-600",
                    Icon = "faComment"
                },
                new()
                {
                    Id = "bot-handled",
                    Title = "Bot successfully handled query",
                    Time = "15 min ago",
                    Description = "E-commerce Bot resolved shipping question automatically",
                    BgColor = "from-green-500 to-green-600",
                    Icon = "faRobot"
                },
                new()
                {
                    Id = "new-page",
                    Title = "New page connected",
                    Time = "1 hour ago",
                    Description = "Facebook page 'Fashion Store' successfully integrated",
                    BgColor = "from-purple-500 to-purple-600",
                    Icon = "faShareNodes"
                },
                new()
                {
                    Id = "daily-report",
                    Title = "Daily report generated",
                    Time = "3 hours ago",
                    Description = "Your daily analytics report is ready for review",
                    BgColor = "from-pink-500 to-pink-600",
                    Icon = "faChartLine"
                }
            };
        }
    }
}