using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BotFlow.Application.Common.DTOs.Settings;
using BotFlow.Application.Services;
using System.Security.Claims;
using System;

namespace BotFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            ISettingsService settingsService,
            ILogger<SettingsController> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        [HttpGet("workspace")]
        public async Task<IActionResult> GetWorkspaceSettings()
        {
            try
            {
                var userId = GetUserId();
                var settings = await _settingsService.GetWorkspaceSettingsAsync(userId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspace settings");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("workspace")]
        public async Task<IActionResult> UpdateWorkspaceSettings([FromBody] WorkspaceSettingsRequest request)
        {
            try
            {
                var userId = GetUserId();
                var settings = await _settingsService.UpdateWorkspaceSettingsAsync(request, userId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workspace settings");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("workspace/logo")]
        public async Task<IActionResult> UploadLogo([FromBody] UploadLogoRequest request)
        {
            try
            {
                var userId = GetUserId();
                var result = await _settingsService.UploadLogoAsync(request, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading logo");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("workspace/logo")]
        public async Task<IActionResult> RemoveLogo()
        {
            try
            {
                var userId = GetUserId();
                var result = await _settingsService.RemoveLogoAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing logo");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotificationSettings()
        {
            try
            {
                var userId = GetUserId();
                var settings = await _settingsService.GetNotificationSettingsAsync(userId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification settings");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("notifications")]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsRequest request)
        {
            try
            {
                var userId = GetUserId();
                var settings = await _settingsService.UpdateNotificationSettingsAsync(request, userId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification settings");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("languages")]
        public async Task<IActionResult> GetAvailableLanguages()
        {
            try
            {
                var languages = new[]
                {
                    new { code = "en", name = "English", nativeName = "English" },
                    new { code = "ar", name = "Arabic", nativeName = "العربية" },
                    new { code = "es", name = "Spanish", nativeName = "Español" },
                    new { code = "fr", name = "French", nativeName = "Français" }
                };
                
                return Ok(languages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available languages");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("timezones")]
        public async Task<IActionResult> GetAvailableTimezones()
        {
            try
            {
                var timezones = new[]
                {
                    new { id = "UTC", name = "UTC +00:00 (GMT)", offset = 0 },
                    new { id = "UTC+3", name = "UTC +03:00 (Arabia Standard Time)", offset = 3 },
                    new { id = "UTC-5", name = "UTC -05:00 (Eastern Time)", offset = -5 },
                    new { id = "UTC-8", name = "UTC -08:00 (Pacific Time)", offset = -8 }
                };
                
                return Ok(timezones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available timezones");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("workspace/delete")]
        public async Task<IActionResult> DeleteWorkspace([FromBody] DeleteWorkspaceRequest request)
        {
            try
            {
                var userId = GetUserId();
                var result = await _settingsService.DeleteWorkspaceAsync(userId, request.Confirmation);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workspace");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("subscription")]
        public async Task<IActionResult> GetSubscriptionInfo()
        {
            try
            {
                var userId = GetUserId();
                var subscription = await _settingsService.GetSubscriptionInfoAsync(userId);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription info");
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

    public class DeleteWorkspaceRequest
    {
        public string Confirmation { get; set; } = string.Empty;
    }
}