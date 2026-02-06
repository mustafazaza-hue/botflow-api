using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BotFlow.Application.Common.DTOs.Pages;
using BotFlow.Application.Services;
using System.Security.Claims;
using System;

namespace BotFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PagesController : ControllerBase
    {
        private readonly IPagesService _pagesService;
        private readonly ILogger<PagesController> _logger;

        public PagesController(
            IPagesService pagesService,
            ILogger<PagesController> logger)
        {
            _pagesService = pagesService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPages()
        {
            try
            {
                var userId = GetUserId();
                var pages = await _pagesService.GetPagesAsync(userId);
                return Ok(pages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pages");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPage(Guid id)
        {
            try
            {
                var page = await _pagesService.GetPageByIdAsync(id);
                return Ok(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page: {PageId}", id);
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("connect")]
        public async Task<IActionResult> ConnectPage([FromBody] ConnectPageRequest request)
        {
            try
            {
                var userId = GetUserId();
                var page = await _pagesService.ConnectPageAsync(request, userId);
                
                return CreatedAtAction(nameof(GetPage), new { id = page.Id }, page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting page");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePage(Guid id, [FromBody] UpdatePageRequest request)
        {
            try
            {
                var page = await _pagesService.UpdatePageAsync(id, request);
                return Ok(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page: {PageId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DisconnectPage(Guid id)
        {
            try
            {
                var result = await _pagesService.DisconnectPageAsync(id);
                if (!result)
                    return NotFound(new { error = "Page not found" });
                
                return Ok(new { message = "Page disconnected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting page: {PageId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/sync")]
        public async Task<IActionResult> SyncPage(Guid id, [FromBody] SyncPageRequest request)
        {
            try
            {
                request.PageId = id; // Ensure consistency
                var result = await _pagesService.SyncPageAsync(request);
                if (!result)
                    return NotFound(new { error = "Page not found" });
                
                return Ok(new { message = "Page synced successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing page: {PageId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/reconnect")]
        public async Task<IActionResult> ReconnectPage(Guid id)
        {
            try
            {
                var result = await _pagesService.ReconnectPageAsync(id);
                if (!result)
                    return NotFound(new { error = "Page not found" });
                
                return Ok(new { message = "Page reconnected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconnecting page: {PageId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("activity-logs")]
        public async Task<IActionResult> GetActivityLogs()
        {
            try
            {
                var userId = GetUserId();
                var logs = await _pagesService.GetActivityLogsAsync(userId);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity logs");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("connection-status")]
        public async Task<IActionResult> GetConnectionStatus()
        {
            try
            {
                var userId = GetUserId();
                var status = await _pagesService.GetConnectionStatusAsync(userId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connection status");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("platforms")]
        public async Task<IActionResult> GetSupportedPlatforms()
        {
            try
            {
                var platforms = new[]
                {
                    new { 
                        id = "facebook", 
                        name = "Facebook", 
                        icon = "fa-facebook", 
                        color = "#1877F2",
                        description = "Connect your Facebook business page"
                    },
                    new { 
                        id = "instagram", 
                        name = "Instagram", 
                        icon = "fa-instagram", 
                        color = "linear-gradient(45deg, #405DE6, #E4405F)",
                        description = "Connect your Instagram business account"
                    }
                };
                
                return Ok(platforms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported platforms");
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