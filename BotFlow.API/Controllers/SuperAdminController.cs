using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BotFlow.Application.Interfaces;
using BotFlow.Application.Common.DTOs.SuperAdmin;
using BotFlow.Application.Common.DTOs.Users;
using System.Security.Claims;

namespace BotFlow.API.Controllers
{
    [Route("api/super-admin")]
    [ApiController]
    [Authorize(Policy = "RequireSuperAdminRole")]
    public class SuperAdminController : ControllerBase
    {
        private readonly IKPIService _kpiService;
        private readonly IAIDataSourceService _aiDataSourceService;
        private readonly IDashboardService _dashboardService;
        private readonly IUserService _userService;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(
            IKPIService kpiService,
            IAIDataSourceService aiDataSourceService,
            IDashboardService dashboardService,
            IUserService userService,
            ILogger<SuperAdminController> logger)
        {
            _kpiService = kpiService;
            _aiDataSourceService = aiDataSourceService;
            _dashboardService = dashboardService;
            _userService = userService;
            _logger = logger;
        }

        // ==================== Dashboard Endpoints ====================
        [HttpGet("dashboard/overview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardOverview()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var overview = await _userService.GetDashboardOverviewAsync();
                return Ok(new
                {
                    Success = true,
                    Message = "Dashboard overview retrieved successfully",
                    Data = overview
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while fetching dashboard overview",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("dashboard/revenue-trend")]
        public async Task<IActionResult> GetRevenueTrend([FromQuery] string period = "monthly")
        {
            try
            {
                var revenueTrend = GetRevenueTrendData(period);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Revenue trend retrieved successfully",
                    Data = revenueTrend
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue trend");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while fetching revenue trend",
                    Error = ex.Message
                });
            }
        }

        private List<object> GetRevenueTrendData(string period)
        {
            var data = new List<object>();
            
            if (period.ToLower() == "monthly")
            {
                data = new List<object>
                {
                    new { Period = "Jan", Amount = 85000, GrowthPercentage = 12.5 },
                    new { Period = "Feb", Amount = 92000, GrowthPercentage = 8.2 },
                    new { Period = "Mar", Amount = 88000, GrowthPercentage = -4.3 },
                    new { Period = "Apr", Amount = 95000, GrowthPercentage = 8.0 },
                    new { Period = "May", Amount = 102000, GrowthPercentage = 7.4 },
                    new { Period = "Jun", Amount = 108000, GrowthPercentage = 5.9 },
                    new { Period = "Jul", Amount = 112000, GrowthPercentage = 3.7 },
                    new { Period = "Aug", Amount = 115000, GrowthPercentage = 2.7 },
                    new { Period = "Sep", Amount = 118000, GrowthPercentage = 2.6 },
                    new { Period = "Oct", Amount = 122000, GrowthPercentage = 3.4 },
                    new { Period = "Nov", Amount = 124800, GrowthPercentage = 2.3 },
                    new { Period = "Dec", Amount = 130000, GrowthPercentage = 4.2 }
                };
            }
            else if (period.ToLower() == "weekly")
            {
                data = new List<object>
                {
                    new { Period = "Week 1", Amount = 28000, GrowthPercentage = 5.2 },
                    new { Period = "Week 2", Amount = 29500, GrowthPercentage = 5.4 },
                    new { Period = "Week 3", Amount = 31200, GrowthPercentage = 5.8 },
                    new { Period = "Week 4", Amount = 32700, GrowthPercentage = 4.8 }
                };
            }
            
            return data;
        }

        [HttpGet("dashboard/subscription-distribution")]
        public async Task<IActionResult> GetSubscriptionDistribution()
        {
            try
            {
                var distribution = await _userService.GetSubscriptionDistributionAsync();
                
                return Ok(new
                {
                    Success = true,
                    Message = "Subscription distribution retrieved successfully",
                    Data = distribution
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription distribution");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while fetching subscription distribution",
                    Error = ex.Message
                });
            }
        }

        // [HttpGet("dashboard/recent-users")]
        // public async Task<IActionResult> GetRecentUsers(
        //     [FromQuery] int page = 1, 
        //     [FromQuery] int pageSize = 10)
        // {
        //     try
        //     {
        //         var users = await _userService.GetRecentUsersAsync(page, pageSize);
                
        //         return Ok(new
        //         {
        //             Success = true,
        //             Message = "Recent users retrieved successfully",
        //             Data = users,
        //             Pagination = new
        //             {
        //                 Page = page,
        //                 PageSize = pageSize,
        //                 TotalCount = users.Count
        //             }
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting recent users");
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while fetching recent users",
        //             Error = ex.Message
        //         });
        //     }
        // }

        // [HttpGet("dashboard/system-performance")]
        // public async Task<IActionResult> GetSystemPerformance()
        // {
        //     try
        //     {
        //         var performance = new
        //         {
        //             ApiResponseTime = 124,
        //             ServerUptime = 99.98,
        //             DatabaseLoad = 62,
        //             BotSuccessRate = 94.2,
        //             ErrorRate = 0.08,
        //             ActiveConnections = 18492,
        //             MemoryUsage = 76.4,
        //             CpuUsage = 42.8,
        //             DiskUsage = 58.3
        //         };
                
        //         return Ok(new
        //         {
        //             Success = true,
        //             Message = "System performance retrieved successfully",
        //             Data = performance
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting system performance");
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while fetching system performance",
        //             Error = ex.Message
        //         });
        //     }
        // }

        // // ==================== User Management Endpoints ====================
        // [HttpGet("users")]
        // public async Task<IActionResult> GetAllUsers([FromQuery] UserFilter filter)
        // {
        //     try
        //     {
        //         var result = await _userService.GetAllUsersAsync(
        //             filter.Page, 
        //             filter.PageSize, 
        //             filter.Search ?? "", 
        //             filter.Role ?? "", 
        //             filter.Status ?? "");

        //         return Ok(new
        //         {
        //             Success = true,
        //             Message = "Users retrieved successfully",
        //             Data = result
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting all users");
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while fetching users",
        //             Error = ex.Message
        //         });
        //     }
        // }

        // [HttpGet("users/{id}")]
        // public async Task<IActionResult> GetUserById(Guid id)
        // {
        //     try
        //     {
        //         var user = await _userService.GetUserByIdAsync(id);
        //         if (user == null)
        //             return NotFound(new { Success = false, Message = "User not found" });

        //         return Ok(new
        //         {
        //             Success = true,
        //             Message = "User retrieved successfully",
        //             Data = user
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting user by ID: {Id}", id);
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while fetching user details",
        //             Error = ex.Message
        //         });
        //     }
        // }

        // [HttpPut("users/{id}")]
        // public async Task<IActionResult> UpdateUser(Guid id, [FromBody] Application.Common.DTOs.SuperAdminUpdateUserRequest request)
        // {
        //     try
        //     {
        //         // تحويل SuperAdminUpdateUserRequest إلى UpdateUserRequest
        //         var updateRequest = new BotFlow.Application.Common.DTOs.Users.UpdateUserRequest
        //         {
        //             FirstName = request.FirstName,
        //             LastName = request.LastName,
        //             Role = request.Role != null ? Enum.Parse<BotFlow.Domain.Enums.UserRole>(request.Role) : (BotFlow.Domain.Enums.UserRole?)null,
        //             SubscriptionPlan = request.SubscriptionPlan,
        //             IsActive = request.IsActive
        //         };

        //         var result = await _userService.UpdateUserAsync(id, updateRequest);
        //         if (!result)
        //             return BadRequest(new { Success = false, Message = "Failed to update user" });

        //         return Ok(new
        //         {
        //             Success = true,
        //             Message = "User updated successfully"
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error updating user: {Id}", id);
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while updating user",
        //             Error = ex.Message
        //         });
        //     }
        // }

        // [HttpPost("users/{id}/suspend")]
        // public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserRequest request)
        // {
        //     try
        //     {
        //         var result = await _userService.SuspendUserAsync(id, request.Reason);
        //         if (!result)
        //             return BadRequest(new { Success = false, Message = "Failed to suspend user" });

        //         return Ok(new 
        //         { 
        //             Success = true, 
        //             Message = "User suspended successfully" 
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error suspending user: {Id}", id);
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while suspending user",
        //             Error = ex.Message
        //         });
        //     }
        // }

        // [HttpPost("users/{id}/activate")]
        // public async Task<IActionResult> ActivateUser(Guid id)
        // {
        //     try
        //     {
        //         var result = await _userService.ActivateUserAsync(id);
        //         if (!result)
        //             return BadRequest(new { Success = false, Message = "Failed to activate user" });

        //         return Ok(new 
        //         { 
        //             Success = true, 
        //             Message = "User activated successfully" 
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error activating user: {Id}", id);
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while activating user",
        //             Error = ex.Message
        //         });
        //     }
        // }

        // [HttpGet("users/stats")]
        // public async Task<IActionResult> GetUserStats()
        // {
        //     try
        //     {
        //         var stats = await _userService.GetUserStatsAsync();
                
        //         return Ok(new
        //         {
        //             Success = true,
        //             Message = "User statistics retrieved successfully",
        //             Data = stats
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting user stats");
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while fetching user statistics",
        //             Error = ex.Message
        //         });
        //     }
        // }

        // ==================== KPI Analytics Endpoints ====================
        // [HttpGet("kpi/overview")]
        // public async Task<IActionResult> GetKPIOverview(
        //     [FromQuery] DateTime? startDate, 
        //     [FromQuery] DateTime? endDate)
        // {
        //     try
        //     {
        //         var overview = new
        //         {
        //             Revenue = 124800,
        //             Users = 2847,
        //             ActiveBots = 8492,
        //             Messages = 42856,
        //             RevenueGrowth = 18.7,
        //             UserGrowth = 12.5,
        //             BotGrowth = 24.1,
        //             MessageGrowth = 15.3
        //         };
                
        //         return Ok(new
        //         {
        //             Success = true,
        //             Message = "KPI overview retrieved successfully",
        //             Data = overview
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting KPI overview");
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while fetching KPI overview",
        //             Error = ex.Message
        //         });
        //     }
        // }

        // [HttpGet("kpi/user-growth")]
        // public async Task<IActionResult> GetUserGrowthTrend([FromQuery] string period = "monthly")
        // {
        //     try
        //     {
        //         List<object> trend;
                
        //         if (period.ToLower() == "monthly")
        //         {
        //             trend = new List<object>
        //             {
        //                 new { Period = "Jan", Users = 2150, Growth = 8.2 },
        //                 new { Period = "Feb", Users = 2280, Growth = 6.0 },
        //                 new { Period = "Mar", Users = 2400, Growth = 5.3 },
        //                 new { Period = "Apr", Users = 2520, Growth = 5.0 },
        //                 new { Period = "May", Users = 2650, Growth = 5.2 },
        //                 new { Period = "Jun", Users = 2750, Growth = 3.8 },
        //                 new { Period = "Jul", Users = 2847, Growth = 3.5 }
        //             };
        //         }
        //         else
        //         {
        //             trend = new List<object>
        //             {
        //                 new { Period = "Week 1", Users = 2420, Growth = 6.1 },
        //                 new { Period = "Week 2", Users = 2567, Growth = 6.1 },
        //                 new { Period = "Week 3", Users = 2698, Growth = 5.1 },
        //                 new { Period = "Week 4", Users = 2847, Growth = 5.5 }
        //             };
        //         }
                
        //         return Ok(new
        //         {
        //             Success = true,
        //             Message = "User growth trend retrieved successfully",
        //             Data = trend
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting user growth trend");
        //         return StatusCode(500, new
        //         {
        //             Success = false,
        //             Message = "An error occurred while fetching user growth trend",
        //             Error = ex.Message
        //         });
        //     }
        // }

        // ... بقية التوابع كما هي ...
    }

    public class UserFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string? Plan { get; set; }
    }

    public class SuspendUserRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}