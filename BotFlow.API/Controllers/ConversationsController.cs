using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BotFlow.Application.Common.DTOs.Conversations;
using BotFlow.Application.Services;
using System.Security.Claims;
using System;

namespace BotFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationsService _conversationsService;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(
            IConversationsService conversationsService,
            ILogger<ConversationsController> logger)
        {
            _conversationsService = conversationsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations([FromQuery] ConversationFilterRequest filter)
        {
            try
            {
                var userId = GetUserId();
                var conversations = await _conversationsService.GetConversationsAsync(filter, userId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConversation(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var conversation = await _conversationsService.GetConversationDetailAsync(id, userId);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation: {ConversationId}", id);
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetMessages(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var messages = await _conversationsService.GetMessagesAsync(id, userId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation: {ConversationId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/messages")]
        public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest request)
        {
            try
            {
                request.ConversationId = id; // Ensure consistency
                var userId = GetUserId();
                var message = await _conversationsService.SendMessageAsync(request, userId);
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to conversation: {ConversationId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _conversationsService.MarkAsReadAsync(id, userId);
                if (!result)
                    return NotFound(new { error = "Conversation not found" });
                
                return Ok(new { message = "Conversation marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation as read: {ConversationId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignConversation(Guid id, [FromBody] AssignRequest request)
        {
            try
            {
                var userId = GetUserId();
                var result = await _conversationsService.AssignConversationAsync(id, request.AssignToUserId, userId);
                if (!result)
                    return NotFound(new { error = "Conversation not found" });
                
                return Ok(new { message = "Conversation assigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning conversation: {ConversationId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/tags")]
        public async Task<IActionResult> AddTag(Guid id, [FromBody] AddTagRequest request)
        {
            try
            {
                var userId = GetUserId();
                var result = await _conversationsService.AddTagAsync(id, request.Tag, userId);
                if (!result)
                    return NotFound(new { error = "Conversation not found" });
                
                return Ok(new { message = $"Tag '{request.Tag}' added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tag to conversation: {ConversationId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}/tags/{tag}")]
        public async Task<IActionResult> RemoveTag(Guid id, string tag)
        {
            try
            {
                var userId = GetUserId();
                var result = await _conversationsService.RemoveTagAsync(id, tag, userId);
                if (!result)
                    return NotFound(new { error = "Conversation not found" });
                
                return Ok(new { message = $"Tag '{tag}' removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tag from conversation: {ConversationId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}/ai-suggestions")]
        public async Task<IActionResult> GetAISuggestions(Guid id)
        {
            try
            {
                var suggestions = await _conversationsService.GetAISuggestionsAsync(id);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI suggestions for conversation: {ConversationId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/export")]
        public async Task<IActionResult> ExportConversation(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _conversationsService.ExportConversationAsync(id, userId);
                if (!result)
                    return NotFound(new { error = "Conversation not found" });
                
                return Ok(new { message = "Conversation exported successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting conversation: {ConversationId}", id);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                var filter = new ConversationFilterRequest
                {
                    Status = "unread",
                    Page = 1,
                    PageSize = 1000
                };
                
                var conversations = await _conversationsService.GetConversationsAsync(filter, userId);
                return Ok(new { count = conversations.Count() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
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

    public class AssignRequest
    {
        public Guid AssignToUserId { get; set; }
    }

    public class AddTagRequest
    {
        public string Tag { get; set; } = string.Empty;
    }
}