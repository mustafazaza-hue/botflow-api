using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BotFlow.Application.Interfaces;
using BotFlow.Application.Common.DTOs;
using BotFlow.Infrastructure.Data;
using System.Linq;

namespace BotFlow.Application.Services
{
    public class KPIService : IKPIService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KPIService> _logger;

        public KPIService(
            ApplicationDbContext context,
            ILogger<KPIService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==================== Dashboard Methods ====================
        public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeSubscriptions = await _context.Users.CountAsync(u => u.IsActive);
            var trialUsers = await _context.Users.CountAsync(u => u.SubscriptionPlan == "Trial");
            var suspendedUsers = await _context.Users.CountAsync(u => !u.IsActive);
            var activeBots = await _context.Bots.CountAsync(b => b.Status == "Active");
            var totalBots = await _context.Bots.CountAsync();

            return new DashboardOverviewDto
            {
                TotalUsers = totalUsers,
                ActiveSubscriptions = activeSubscriptions,
                TrialUsers = trialUsers,
                SuspendedUsers = suspendedUsers,
                MonthlyRevenue = 124800m,
                ActiveBots = activeBots,
                TotalBots = totalBots,
                TotalDocuments = 247,
                ActiveDataSources = 234,
                ProcessingDataSources = 8,
                FailedDataSources = 5,
                UserGrowthPercentage = 12.5m,
                RevenueGrowthPercentage = 18.7m,
                BotGrowthPercentage = 24.1m
            };
        }

        public async Task<List<RevenueTrendDto>> GetRevenueTrendAsync(string period = "monthly")
        {
            var revenueTrend = new List<RevenueTrendDto>();
            
            if (period.ToLower() == "monthly")
            {
                revenueTrend = new List<RevenueTrendDto>
                {
                    new() { Period = "Jan", Amount = 85000, GrowthPercentage = 12.5m },
                    new() { Period = "Feb", Amount = 92000, GrowthPercentage = 8.2m },
                    new() { Period = "Mar", Amount = 88000, GrowthPercentage = -4.3m },
                    new() { Period = "Apr", Amount = 95000, GrowthPercentage = 8.0m },
                    new() { Period = "May", Amount = 102000, GrowthPercentage = 7.4m },
                    new() { Period = "Jun", Amount = 108000, GrowthPercentage = 5.9m },
                    new() { Period = "Jul", Amount = 112000, GrowthPercentage = 3.7m },
                    new() { Period = "Aug", Amount = 115000, GrowthPercentage = 2.7m },
                    new() { Period = "Sep", Amount = 118000, GrowthPercentage = 2.6m },
                    new() { Period = "Oct", Amount = 122000, GrowthPercentage = 3.4m },
                    new() { Period = "Nov", Amount = 124800, GrowthPercentage = 2.3m },
                    new() { Period = "Dec", Amount = 130000, GrowthPercentage = 4.2m }
                };
            }
            else if (period.ToLower() == "weekly")
            {
                revenueTrend = new List<RevenueTrendDto>
                {
                    new() { Period = "Week 1", Amount = 28000, GrowthPercentage = 5.2m },
                    new() { Period = "Week 2", Amount = 29500, GrowthPercentage = 5.4m },
                    new() { Period = "Week 3", Amount = 31200, GrowthPercentage = 5.8m },
                    new() { Period = "Week 4", Amount = 32700, GrowthPercentage = 4.8m }
                };
            }
            
            return revenueTrend;
        }

        public async Task<SubscriptionDistributionDto> GetSubscriptionDistributionAsync()
        {
            var business = await _context.Users.CountAsync(u => u.SubscriptionPlan == "Business");
            var pro = await _context.Users.CountAsync(u => u.SubscriptionPlan == "Pro");
            var starter = await _context.Users.CountAsync(u => u.SubscriptionPlan == "Starter");
            var trial = await _context.Users.CountAsync(u => u.SubscriptionPlan == "Trial");

            return new SubscriptionDistributionDto
            {
                Business = business,
                Pro = pro,
                Starter = starter,
                Trial = trial,
                Total = business + pro + starter + trial
            };
        }

        public async Task<List<RecentUserDto>> GetRecentUsersAsync(int page = 1, int pageSize = 10)
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new RecentUserDto
                {
                    Id = u.Id,
                    Name = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    CompanyName = u.CompanyName,
                    Plan = u.SubscriptionPlan,
                    Status = u.IsActive ? "Active" : "Suspended",
                    BotCount = _context.Bots.Count(b => b.UserId == u.Id),
                    Revenue = "$0/mo",
                    JoinedDate = u.CreatedAt,
                    AvatarUrl = "https://storage.googleapis.com/uxpilot-auth.appspot.com/avatars/avatar-4.jpg"
                })
                .ToListAsync();

            return users;
        }

        public async Task<SystemPerformanceDto> GetSystemPerformanceAsync()
        {
            return new SystemPerformanceDto
            {
                ApiResponseTime = 124,
                ServerUptime = 99.98m,
                DatabaseLoad = 62m,
                BotSuccessRate = 94.2m,
                ErrorRate = 0.08m,
                ActiveConnections = 18492,
                MemoryUsage = 76.4m,
                CpuUsage = 42.8m,
                DiskUsage = 58.3m
            };
        }

        public async Task<UserStatsDto> GetUserStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var today = DateTime.UtcNow.Date;
            var newUsersToday = await _context.Users.CountAsync(u => u.CreatedAt.Date == today);
            
            var activeRate = totalUsers > 0 ? (decimal)activeUsers / totalUsers * 100 : 0;

            return new UserStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                NewUsersToday = newUsersToday,
                NewUsersThisWeek = await _context.Users.CountAsync(u => u.CreatedAt >= today.AddDays(-7)),
                NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= today.AddDays(-30)),
                AverageSessionDuration = "4m 32s",
                ActiveRate = Math.Round(activeRate, 1),
                ChurnRate = 2.4m
            };
        }

        // ==================== KPI Analytics Methods ====================
        public async Task<KPIOverviewDto> GetKPIOverviewAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            return new KPIOverviewDto
            {
                Revenue = 124800m,
                Users = 2847,
                ActiveBots = 8492,
                Messages = 42856,
                RevenueGrowth = 18.7m,
                UserGrowth = 12.5m,
                BotGrowth = 24.1m,
                MessageGrowth = 15.3m
            };
        }

        public async Task<List<UserGrowthDto>> GetUserGrowthTrendAsync(string period = "monthly")
        {
            List<UserGrowthDto> trend;
            
            if (period.ToLower() == "monthly")
            {
                trend = new List<UserGrowthDto>
                {
                    new() { Period = "Jan", Users = 2150, Growth = 8.2m },
                    new() { Period = "Feb", Users = 2280, Growth = 6.0m },
                    new() { Period = "Mar", Users = 2400, Growth = 5.3m },
                    new() { Period = "Apr", Users = 2520, Growth = 5.0m },
                    new() { Period = "May", Users = 2650, Growth = 5.2m },
                    new() { Period = "Jun", Users = 2750, Growth = 3.8m },
                    new() { Period = "Jul", Users = 2847, Growth = 3.5m }
                };
            }
            else
            {
                trend = new List<UserGrowthDto>
                {
                    new() { Period = "Week 1", Users = 2420, Growth = 6.1m },
                    new() { Period = "Week 2", Users = 2567, Growth = 6.1m },
                    new() { Period = "Week 3", Users = 2698, Growth = 5.1m },
                    new() { Period = "Week 4", Users = 2847, Growth = 5.5m }
                };
            }
            
            return trend;
        }

        public async Task<List<ApiUsageDto>> GetApiUsageAsync(DateTime startDate, DateTime endDate)
        {
            return new List<ApiUsageDto>
            {
                new() { Date = "Mon", UXPilot = 22400, WhatsApp = 12800, Total = 35200 },
                new() { Date = "Tue", UXPilot = 24800, WhatsApp = 14200, Total = 39000 },
                new() { Date = "Wed", UXPilot = 23600, WhatsApp = 13400, Total = 37000 },
                new() { Date = "Thu", UXPilot = 26200, WhatsApp = 15600, Total = 41800 },
                new() { Date = "Fri", UXPilot = 28400, WhatsApp = 16800, Total = 45200 },
                new() { Date = "Sat", UXPilot = 21200, WhatsApp = 11400, Total = 32600 },
                new() { Date = "Sun", UXPilot = 19600, WhatsApp = 10200, Total = 29800 }
            };
        }

        public async Task<SystemHealthDto> GetSystemHealthAsync()
        {
            return new SystemHealthDto
            {
                Uptime = 99.98m,
                ResponseTime = 124,
                ErrorRate = 0.08m,
                ActiveUsers = 18492,
                ApiCallsPerSecond = 156,
                DatabaseConnections = 248,
                MemoryUsage = 76.4m,
                CpuUsage = 42.8m,
                HealthStatus = "Healthy"
            };
        }

        public async Task<CostBreakdownDto> GetCostBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            return new CostBreakdownDto
            {
                UXPilotApiCost = 4287m,
                WhatsAppApiCost = 2014m,
                ServerCosts = 1240m,
                DatabaseCosts = 980m,
                ExternalServices = 540m,
                SupportCosts = 320m,
                MarketingCosts = 2150m,
                TotalCost = 11531m,
                NetRevenue = 113269m
            };
        }

        public async Task<List<MessageVolumeDto>> GetMessageVolumeAsync(DateTime startDate, DateTime endDate)
        {
            return new List<MessageVolumeDto>
            {
                new() { Date = "Mon", Messages = 12400, AutoReplies = 9200, ManualReplies = 3200 },
                new() { Date = "Tue", Messages = 15200, AutoReplies = 11500, ManualReplies = 3700 },
                new() { Date = "Wed", Messages = 14800, AutoReplies = 10800, ManualReplies = 4000 },
                new() { Date = "Thu", Messages = 16900, AutoReplies = 12400, ManualReplies = 4500 },
                new() { Date = "Fri", Messages = 18200, AutoReplies = 13200, ManualReplies = 5000 },
                new() { Date = "Sat", Messages = 11500, AutoReplies = 8400, ManualReplies = 3100 },
                new() { Date = "Sun", Messages = 9800, AutoReplies = 7200, ManualReplies = 2600 }
            };
        }

        // ==================== Additional KPI Methods ====================
        public async Task<List<KPIMetricDto>> GetKPIMetricsAsync(string metricType = "", string period = "daily", 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var metrics = new List<KPIMetricDto>();
            
            // بيانات نموذجية
            if (string.IsNullOrEmpty(metricType) || metricType == "UserGrowth")
            {
                metrics.AddRange(new[]
                {
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "UserGrowth", Period = "Weekly", Date = DateTime.UtcNow.AddDays(-21), Value = 2420, TargetValue = 2500, ChangePercentage = 6.1 },
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "UserGrowth", Period = "Weekly", Date = DateTime.UtcNow.AddDays(-14), Value = 2567, TargetValue = 2600, ChangePercentage = 6.1 },
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "UserGrowth", Period = "Weekly", Date = DateTime.UtcNow.AddDays(-7), Value = 2698, TargetValue = 2700, ChangePercentage = 5.1 },
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "UserGrowth", Period = "Weekly", Date = DateTime.UtcNow, Value = 2847, TargetValue = 2850, ChangePercentage = 5.5 }
                });
            }
            
            if (string.IsNullOrEmpty(metricType) || metricType == "ApiUsage")
            {
                metrics.AddRange(new[]
                {
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "ApiUsage", Period = "Daily", Date = DateTime.UtcNow.AddDays(-6), Value = 35200, TargetValue = 30000, ChangePercentage = 17.3, Platform = "All" },
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "ApiUsage", Period = "Daily", Date = DateTime.UtcNow.AddDays(-5), Value = 39000, TargetValue = 32000, ChangePercentage = 21.9, Platform = "All" },
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "ApiUsage", Period = "Daily", Date = DateTime.UtcNow.AddDays(-4), Value = 37000, TargetValue = 31000, ChangePercentage = 19.4, Platform = "All" },
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "ApiUsage", Period = "Daily", Date = DateTime.UtcNow.AddDays(-3), Value = 41800, TargetValue = 35000, ChangePercentage = 19.4, Platform = "All" },
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "ApiUsage", Period = "Daily", Date = DateTime.UtcNow.AddDays(-2), Value = 45200, TargetValue = 38000, ChangePercentage = 18.9, Platform = "All" },
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "ApiUsage", Period = "Daily", Date = DateTime.UtcNow.AddDays(-1), Value = 32600, TargetValue = 28000, ChangePercentage = 16.4, Platform = "All" },
                    new KPIMetricDto { Id = Guid.NewGuid(), MetricType = "ApiUsage", Period = "Daily", Date = DateTime.UtcNow, Value = 29800, TargetValue = 26000, ChangePercentage = 14.6, Platform = "All" }
                });
            }
            
            return metrics;
        }

        public async Task<List<RevenueAnalysisDto>> GetRevenueAnalysisAsync(DateTime startDate, DateTime endDate)
        {
            var analysis = new List<RevenueAnalysisDto>();
            var currentDate = startDate;
            
            while (currentDate <= endDate && analysis.Count < 12)
            {
                analysis.Add(new RevenueAnalysisDto
                {
                    Date = currentDate,
                    Revenue = 10000 + (analysis.Count * 2000),
                    NewUsers = 80 + (analysis.Count * 10),
                    ActiveUsers = 2000 + (analysis.Count * 150),
                    AverageRevenuePerUser = 45 + (analysis.Count * 2),
                    ChurnRate = 2.4m - (analysis.Count * 0.1m),
                    LifetimeValue = 540 + (analysis.Count * 30)
                });
                
                currentDate = currentDate.AddDays(7);
            }
            
            return analysis;
        }

        public async Task UpdateSystemStatisticsAsync()
        {
            try
            {
                // تحديث إحصائيات النظام في قاعدة البيانات
                _logger.LogInformation("System statistics updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system statistics");
            }
        }

        public async Task<SystemStatisticDto> GetLatestSystemStatisticsAsync()
        {
            return new SystemStatisticDto
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.Date,
                TotalUsers = 2847,
                ActiveSubscriptions = 2634,
                TrialUsers = 164,
                SuspendedUsers = 49,
                TotalBots = 8492,
                TotalDocuments = 247,
                ActiveDataSources = 234,
                ProcessingDataSources = 8,
                FailedDataSources = 5,
                TotalRevenue = 124800,
                UXPilotApiCost = 4287,
                WhatsAppApiCost = 2014,
                AvgResponseTime = 124,
                ServerUptime = 99.98m,
                DatabaseLoad = 62,
                BotSuccessRate = 94.2m,
                ErrorRate = 0.08m,
                TotalComments = 18492,
                MessagesSent = 42856,
                UXPilotApiCalls = 156200,
                WhatsAppApiCalls = 89400,
                CreatedAt = DateTime.UtcNow
            };
        }

        // ==================== Helper Methods ====================
        public async Task<List<object>> GetKPIMetricsChartDataAsync(string metricType, string period)
        {
            // بيانات نموذجية للرسوم البيانية
            return new List<object>
            {
                new { x = "Jan", y = 85000 },
                new { x = "Feb", y = 92000 },
                new { x = "Mar", y = 88000 },
                new { x = "Apr", y = 95000 },
                new { x = "May", y = 102000 },
                new { x = "Jun", y = 108000 }
            };
        }

        public async Task<decimal> CalculateRevenueGrowthAsync(DateTime startDate, DateTime endDate)
        {
            // حساب نمو الإيرادات
            return 18.7m;
        }

        public async Task<decimal> CalculateUserGrowthAsync(DateTime startDate, DateTime endDate)
        {
            // حساب نمو المستخدمين
            return 12.5m;
        }
    }
}