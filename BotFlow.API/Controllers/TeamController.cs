using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BotFlow.Application.Common.DTOs.Team;
using BotFlow.Application.Services;
using System.Security.Claims;
using System;

namespace BotFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly ILogger<TeamController> _logger;

        public TeamController(
            ITeamService teamService,
            ILogger<TeamController> logger)
        {
            _teamService = teamService;
            _logger = logger;
        }

        [HttpGet("members")]
        public async Task<IActionResult> GetTeamMembers()
        {
            try
            {
                var userId = GetUserId();
                var members = await _teamService.GetTeamMembersAsync(userId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team members");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetTeamStats()
        {
            try
            {
                var userId = GetUserId();
                var stats = await _teamService.GetTeamStatsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team stats");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("invite")]
        public async Task<IActionResult> InviteMember([FromBody] InviteMemberRequest request)
        {
            try
            {
                var userId = GetUserId();
                var result = await _teamService.InviteMemberAsync(request, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting team member");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("members/{id}/role")]
        public async Task<IActionResult> UpdateMemberRole(Guid id, [FromBody] UpdateMemberRoleRequest request)
        {
            try
            {
                request.MemberId = id; // Ensure consistency
                var userId = GetUserId();
                var result = await _teamService.UpdateMemberRoleAsync(request, userId);
                if (!result)
                    return NotFound(new { error = "Team member not found" });
                
                return Ok(new { message = "Member role updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member role: {MemberId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("members/{id}")]
        public async Task<IActionResult> RemoveMember(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _teamService.RemoveMemberAsync(id, userId);
                if (!result)
                    return NotFound(new { error = "Team member not found" });
                
                return Ok(new { message = "Team member removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing team member: {MemberId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("activity-logs")]
        public async Task<IActionResult> GetActivityLogs()
        {
            try
            {
                var userId = GetUserId();
                var logs = await _teamService.GetActivityLogsAsync(userId);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity logs");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetAvailableRoles()
        {
            try
            {
                var roles = new[]
                {
                    new { 
                        id = "owner", 
                        name = "Owner", 
                        description = "Full access to all features and settings",
                        permissions = new[] { "all" },
                        icon = "fa-crown",
                        color = "bg-purple-100 text-purple-700"
                    },
                    new { 
                        id = "admin", 
                        name = "Admin", 
                        description = "Can manage bots, pages, and team members",
                        permissions = new[] { "bots", "pages", "team", "analytics" },
                        icon = "fa-user-shield",
                        color = "bg-blue-100 text-blue-700"
                    },
                    new { 
                        id = "agent", 
                        name = "Agent", 
                        description = "Can manage conversations and respond to users",
                        permissions = new[] { "conversations", "messages" },
                        icon = "fa-headset",
                        color = "bg-emerald-100 text-emerald-700"
                    },
                    new { 
                        id = "viewer", 
                        name = "Viewer", 
                        description = "Can view analytics and reports",
                        permissions = new[] { "analytics", "reports" },
                        icon = "fa-eye",
                        color = "bg-gray-100 text-gray-700"
                    }
                };
                
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available roles");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("members/{id}/resend-invite")]
        public async Task<IActionResult> ResendInvite(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _teamService.ResendInviteAsync(id, userId);
                if (!result)
                    return NotFound(new { error = "Team member not found" });
                
                return Ok(new { message = "Invitation resent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending invite: {MemberId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("pending-invites")]
        public async Task<IActionResult> GetPendingInvites()
        {
            try
            {
                var userId = GetUserId();
                var invites = await _teamService.GetPendingInvitesAsync(userId);
                return Ok(invites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending invites");
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