using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BotFlow.Application.Common.DTOs;
using BotFlow.Application.Interfaces;
using BotFlow.API.Extensions;

namespace BotFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet("metrics")]
        [ProducesResponseType(typeof(DashboardMetricsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DashboardMetricsDto>> GetDashboardMetrics()
        {
            try
            {
                var userId = User.GetUserId();
                _logger.LogInformation("Getting dashboard metrics for user {UserId}", userId);
                
                var metrics = await _dashboardService.GetDashboardMetricsAsync(userId);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard metrics");
                return StatusCode(500, new { message = "An error occurred while fetching dashboard metrics." });
            }
        }

        [HttpGet("conversation-trend")]
        [ProducesResponseType(typeof(ConversationTrendDto), 200)]
        public async Task<ActionResult<ConversationTrendDto>> GetConversationTrend(
            [FromQuery] string timeRange = "weekly")
        {
            try
            {
                var userId = User.GetUserId();
                var trend = await _dashboardService.GetConversationTrendAsync(userId, timeRange);
                return Ok(trend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation trend");
                return StatusCode(500, new { message = "An error occurred while fetching conversation trend." });
            }
        }

        [HttpGet("response-times")]
        [ProducesResponseType(typeof(ResponseTimeDto), 200)]
        public async Task<ActionResult<ResponseTimeDto>> GetResponseTimes()
        {
            try
            {
                var userId = User.GetUserId();
                var responseTimes = await _dashboardService.GetResponseTimesAsync(userId);
                return Ok(responseTimes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting response times");
                return StatusCode(500, new { message = "An error occurred while fetching response times." });
            }
        }

        [HttpGet("engagement-sources")]
        [ProducesResponseType(typeof(EngagementSourcesDto), 200)]
        public async Task<ActionResult<EngagementSourcesDto>> GetEngagementSources()
        {
            try
            {
                var userId = User.GetUserId();
                var sources = await _dashboardService.GetEngagementSourcesAsync(userId);
                return Ok(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting engagement sources");
                return StatusCode(500, new { message = "An error occurred while fetching engagement sources." });
            }
        }

        [HttpGet("recent-activities")]
        [ProducesResponseType(typeof(List<RecentActivityDto>), 200)]
        public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivities(
            [FromQuery] int count = 10)
        {
            try
            {
                var userId = User.GetUserId();
                var activities = await _dashboardService.GetRecentActivitiesAsync(userId, count);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
                return StatusCode(500, new { message = "An error occurred while fetching recent activities." });
            }
        }

        [HttpGet("alerts")]
        [ProducesResponseType(typeof(DashboardAlertDto), 200)]
        public async Task<ActionResult<DashboardAlertDto>> GetAlerts()
        {
            try
            {
                var userId = User.GetUserId();
                var alerts = await _dashboardService.GetAlertsAsync(userId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts");
                return StatusCode(500, new { message = "An error occurred while fetching alerts." });
            }
        }

        [HttpPost("export")]
        [ProducesResponseType(typeof(DashboardExportDto), 200)]
        public async Task<IActionResult> ExportDashboardData([FromBody] TimeRangeDto timeRange)
        {
            try
            {
                var userId = User.GetUserId();
                _logger.LogInformation("Exporting dashboard data for user {UserId}", userId);
                
                var exportResult = await _dashboardService.ExportDashboardDataAsync(userId, timeRange);
                return Ok(exportResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting dashboard data");
                return StatusCode(500, new { message = "An error occurred while exporting data." });
            }
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult<object>> GetDashboardSummary()
        {
            try
            {
                var userId = User.GetUserId();
                var metrics = await _dashboardService.GetDashboardMetricsAsync(userId);
                var alerts = await _dashboardService.GetAlertsAsync(userId);

                return Ok(new
                {
                    Metrics = metrics,
                    Alerts = alerts,
                    LastUpdated = DateTime.UtcNow,
                    UserId = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, new { message = "An error occurred while fetching dashboard summary." });
            }
        }
    }
}