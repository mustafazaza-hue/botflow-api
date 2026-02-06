using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BotFlow.Application.Common.DTOs.Analytics;
using BotFlow.Application.Services;
using System.Security.Claims;
using System;

namespace BotFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics([FromQuery] AnalyticsFilterRequest filter)
        {
            try
            {
                var userId = GetUserId();
                var metrics = await _analyticsService.GetMetricsAsync(filter);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics metrics");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("top-bots")]
        public async Task<IActionResult> GetTopBots([FromQuery] AnalyticsFilterRequest filter)
        {
            try
            {
                var topBots = await _analyticsService.GetTopBotsAsync(filter);
                return Ok(topBots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top bots");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("top-pages")]
        public async Task<IActionResult> GetTopPages([FromQuery] AnalyticsFilterRequest filter)
        {
            try
            {
                var topPages = await _analyticsService.GetTopPagesAsync(filter);
                return Ok(topPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top pages");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("messages-chart")]
        public async Task<IActionResult> GetMessagesChart([FromQuery] AnalyticsFilterRequest filter)
        {
            try
            {
                var chartData = await _analyticsService.GetMessagesChartAsync(filter);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages chart");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("engagement-chart")]
        public async Task<IActionResult> GetEngagementChart([FromQuery] AnalyticsFilterRequest filter)
        {
            try
            {
                var chartData = await _analyticsService.GetEngagementChartAsync(filter);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting engagement chart");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("response-time-chart")]
        public async Task<IActionResult> GetResponseTimeChart([FromQuery] AnalyticsFilterRequest filter)
        {
            try
            {
                var chartData = await _analyticsService.GetResponseTimeChartAsync(filter);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting response time chart");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("conversion-chart")]
        public async Task<IActionResult> GetConversionChart([FromQuery] AnalyticsFilterRequest filter)
        {
            try
            {
                var chartData = await _analyticsService.GetConversionChartAsync(filter);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversion chart");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("time-series")]
        public async Task<IActionResult> GetTimeSeriesData([FromQuery] AnalyticsFilterRequest filter)
        {
            try
            {
                var data = await _analyticsService.GetTimeSeriesDataAsync(filter);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting time series data");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportAnalytics([FromBody] AnalyticsFilterRequest filter)
        {
            try
            {
                // In production, this would generate and return a file
                var exportData = new
                {
                    Message = "Export functionality will be implemented soon",
                    Filter = filter,
                    GeneratedAt = DateTime.UtcNow
                };
                
                return Ok(exportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting analytics");
                return BadRequest(new { error = ex.Message });
            }
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID");
            
            return userId;
        }
    }
}