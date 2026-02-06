using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BotFlow.Application.Interfaces;
using System.Security.Claims;

namespace BotFlow.API.Controllers.SuperAdmin
{
    [ApiController]
    [Route("api/super-admin/[controller]")]
    [Authorize(Policy = "RequireSuperAdminRole")]
    public class KPIController : ControllerBase
    {
        private readonly IKPIService _kpiService;

        public KPIController(IKPIService kpiService)
        {
            _kpiService = kpiService;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetKPIOverview([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var overview = await _kpiService.GetKPIOverviewAsync(startDate.Value, endDate.Value);
                
                return Ok(new
                {
                    Success = true,
                    Message = "KPI overview retrieved successfully",
                    Data = overview
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error retrieving KPI overview",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetKPIMetrics(
            [FromQuery] string metricType = "",
            [FromQuery] string period = "daily",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var metrics = await _kpiService.GetKPIMetricsAsync(metricType, period, startDate, endDate);
                
                return Ok(new
                {
                    Success = true,
                    Message = "KPI metrics retrieved successfully",
                    Data = metrics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error retrieving KPI metrics",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("revenue-analysis")]
        public async Task<IActionResult> GetRevenueAnalysis([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-90);
                endDate ??= DateTime.UtcNow;

                var analysis = await _kpiService.GetRevenueAnalysisAsync(startDate.Value, endDate.Value);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Revenue analysis retrieved successfully",
                    Data = analysis
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error retrieving revenue analysis",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("user-growth")]
        public async Task<IActionResult> GetUserGrowthTrend([FromQuery] string period = "monthly")
        {
            try
            {
                var trend = await _kpiService.GetUserGrowthTrendAsync(period);
                
                return Ok(new
                {
                    Success = true,
                    Message = "User growth trend retrieved successfully",
                    Data = trend
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error retrieving user growth trend",
                    Error = ex.Message
                });
            }
        }
    }
}