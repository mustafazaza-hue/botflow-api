using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BotFlow.Application.Common.DTOs.Bots;
using BotFlow.Application.Services;
using System.Security.Claims;
using System;

namespace BotFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BotsController : ControllerBase
    {
        private readonly IBotsService _botsService;
        private readonly ILogger<BotsController> _logger;

        public BotsController(
            IBotsService botsService,
            ILogger<BotsController> logger)
        {
            _botsService = botsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetBots([FromQuery] BotFilterRequest filter)
        {
            try
            {
                var userId = GetUserId();
                var bots = await _botsService.GetBotsAsync(filter);
                return Ok(bots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bots");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBot(Guid id)
        {
            try
            {
                var bot = await _botsService.GetBotByIdAsync(id);
                return Ok(bot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bot: {BotId}", id);
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var userId = GetUserId();
                var stats = await _botsService.GetBotsStatsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bot stats");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBot([FromBody] CreateBotRequest request)
        {
            try
            {
                var userId = GetUserId();
                var bot = await _botsService.CreateBotAsync(request, userId);
                
                return CreatedAtAction(nameof(GetBot), new { id = bot.Id }, bot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bot");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBot(Guid id, [FromBody] UpdateBotRequest request)
        {
            try
            {
                var bot = await _botsService.UpdateBotAsync(id, request);
                return Ok(bot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bot: {BotId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBot(Guid id)
        {
            try
            {
                var result = await _botsService.DeleteBotAsync(id);
                if (!result)
                    return NotFound(new { error = "Bot not found" });
                
                return Ok(new { message = "Bot deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bot: {BotId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
        {
            try
            {
                var result = await _botsService.ChangeBotStatusAsync(id, request.Status);
                if (!result)
                    return NotFound(new { error = "Bot not found" });
                
                return Ok(new { message = $"Bot status changed to {request.Status}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing bot status: {BotId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBots([FromQuery] string query)
        {
            try
            {
                var userId = GetUserId();
                var bots = await _botsService.SearchBotsAsync(query, userId);
                return Ok(bots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching bots");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/duplicate")]
        public async Task<IActionResult> DuplicateBot(Guid id)
        {
            try
            {
                var bot = await _botsService.GetBotByIdAsync(id);
                var duplicateRequest = new CreateBotRequest
                {
                    Name = $"{bot.Name} (Copy)",
                    Description = bot.Description,
                    FlowConfiguration = "{}", // Would copy the actual configuration in production
                    WelcomeMessage = "Welcome! How can I help you today?",
                    FallbackMessage = "I didn't understand that. Could you please rephrase?",
                    IsAutoResponder = true
                };
                
                var userId = GetUserId();
                var newBot = await _botsService.CreateBotAsync(duplicateRequest, userId);
                
                return CreatedAtAction(nameof(GetBot), new { id = newBot.Id }, newBot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating bot: {BotId}", id);
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

    public class ChangeStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}