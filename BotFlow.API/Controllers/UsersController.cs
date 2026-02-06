using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BotFlow.Application.Interfaces;
using BotFlow.Application.Common.DTOs.Users; // أضف هذا
using System.Security.Claims;

namespace BotFlow.API.Controllers.SuperAdmin
{
    [ApiController]
    [Route("api/super-admin/[controller]")]
    [Authorize(Policy = "RequireSuperAdminRole")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string search = "",
            [FromQuery] string role = "",
            [FromQuery] string status = "")
        {
            try
            {
                var result = await _userService.GetAllUsersAsync(page, pageSize, search, role, status);
                
                return Ok(new
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = result.Users,
                    Pagination = new
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = result.TotalCount,
                        TotalPages = result.TotalPages
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error retrieving users",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                
                if (user == null)
                    return NotFound(new
                    {
                        Success = false,
                        Message = "User not found"
                    });

                return Ok(new
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error retrieving user",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto request)
        {
            try
            {
                // تحويل UpdateUserDto إلى UpdateUserRequest
                var updateRequest = new BotFlow.Application.Common.DTOs.Users.UpdateUserRequest
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = request.Role != null ? Enum.Parse<BotFlow.Domain.Enums.UserRole>(request.Role) : (BotFlow.Domain.Enums.UserRole?)null,
                    SubscriptionPlan = request.SubscriptionPlan,
                    IsActive = request.IsActive
                };

                var result = await _userService.UpdateUserAsync(id, updateRequest);
                
                if (!result)
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Failed to update user"
                    });

                return Ok(new
                {
                    Success = true,
                    Message = "User updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error updating user",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("{id}/suspend")]
        public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserRequest request)
        {
            try
            {
                var result = await _userService.SuspendUserAsync(id, request.Reason);
                
                if (!result)
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Failed to suspend user"
                    });

                return Ok(new
                {
                    Success = true,
                    Message = "User suspended successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error suspending user",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            try
            {
                var result = await _userService.ActivateUserAsync(id);
                
                if (!result)
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Failed to activate user"
                    });

                return Ok(new
                {
                    Success = true,
                    Message = "User activated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error activating user",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetUserStatistics()
        {
            try
            {
                // هذا endpoint ليس له معنى بدون معرف المستخدم
                // يمكنك إزالته أو تعديله ليعيد إحصائيات عامة
                return BadRequest(new
                {
                    Success = false,
                    Message = "This endpoint requires a user ID"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error retrieving user statistics",
                    Error = ex.Message
                });
            }
        }

        // إضافة endpoint صحيح لـ statistics مع معرف المستخدم
        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetUserStatistics(Guid id)
        {
            try
            {
                var stats = await _userService.GetUserStatisticsAsync(id);
                
                return Ok(new
                {
                    Success = true,
                    Message = "User statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error retrieving user statistics",
                    Error = ex.Message
                });
            }
        }
    }

    // غير اسم الـ DTO لتجنب التضارب
    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        public string? SubscriptionPlan { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SuspendUserRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}