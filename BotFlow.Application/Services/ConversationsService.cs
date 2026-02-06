using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BotFlow.Application.Common.DTOs.Conversations;
using BotFlow.Application.Interfaces;
using BotFlow.Domain.Entities;
using BotFlow.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFlow.Application.Services
{
    public interface IConversationsService
    {
        Task<IEnumerable<ConversationResponse>> GetConversationsAsync(ConversationFilterRequest filter, Guid userId);
        Task<ConversationDetailResponse> GetConversationDetailAsync(Guid id, Guid userId);
        Task<IEnumerable<MessageResponse>> GetMessagesAsync(Guid conversationId, Guid userId);
        Task<MessageResponse> SendMessageAsync(SendMessageRequest request, Guid userId);
        Task<bool> MarkAsReadAsync(Guid conversationId, Guid userId);
        Task<bool> AssignConversationAsync(Guid conversationId, Guid assignToUserId, Guid userId);
        Task<bool> AddTagAsync(Guid conversationId, string tag, Guid userId);
        Task<bool> RemoveTagAsync(Guid conversationId, string tag, Guid userId);
        Task<IEnumerable<string>> GetAISuggestionsAsync(Guid conversationId);
        Task<bool> ExportConversationAsync(Guid conversationId, Guid userId);
    }

    public class ConversationsService : IConversationsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConversationsService> _logger;
        private readonly IEmailService _emailService;

        public ConversationsService(
            ApplicationDbContext context,
            ILogger<ConversationsService> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<IEnumerable<ConversationResponse>> GetConversationsAsync(ConversationFilterRequest filter, Guid userId)
        {
            try
            {
                var query = _context.Conversations
                    .Include(c => c.Tags)
                    .Include(c => c.AssignedTo)
                    .Where(c => c.Bot.UserId == userId || c.Page.UserId == userId);

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "all")
                {
                    query = filter.Status switch
                    {
                        "unread" => query.Where(c => !c.IsRead),
                        "assigned" => query.Where(c => c.IsAssigned),
                        _ => query
                    };
                }

                if (!string.IsNullOrEmpty(filter.Platform))
                    query = query.Where(c => c.Platform == filter.Platform);

                if (filter.PageId.HasValue)
                    query = query.Where(c => c.PageId == filter.PageId);

                if (filter.AssignedTo.HasValue)
                    query = query.Where(c => c.AssignedToUserId == filter.AssignedTo);

                if (filter.Tags != null && filter.Tags.Any())
                    query = query.Where(c => c.Tags.Any(t => filter.Tags.Contains(t.Tag)));

                if (!string.IsNullOrEmpty(filter.SearchQuery))
                    query = query.Where(c => 
                        c.UserName.Contains(filter.SearchQuery) || 
                        c.Messages.Any(m => m.Content.Contains(filter.SearchQuery)));

                // Apply sorting and pagination
                var conversations = await query
                    .OrderByDescending(c => c.LastMessageAt)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return conversations.Select(MapToConversationResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<ConversationDetailResponse> GetConversationDetailAsync(Guid id, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Tags)
                    .Include(c => c.AssignedTo)
                    .Include(c => c.Messages)
                    .Include(c => c.Page)
                    .FirstOrDefaultAsync(c => c.Id == id && 
                        (c.Bot.UserId == userId || c.Page.UserId == userId));

                if (conversation == null)
                    throw new ApplicationException("Conversation not found");

                // Mark as read
                if (!conversation.IsRead)
                {
                    conversation.IsRead = true;
                    await _context.SaveChangesAsync();
                }

                var messages = await GetMessagesAsync(id, userId);
                var contactInfo = await GetContactInfoAsync(conversation);

                return new ConversationDetailResponse
                {
                    Conversation = MapToConversationResponse(conversation),
                    Messages = messages.ToArray(),
                    ContactInfo = contactInfo,
                    QuickActions = GetQuickActions(conversation)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation detail: {ConversationId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<MessageResponse>> GetMessagesAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId && 
                        (c.Bot.UserId == userId || c.Page.UserId == userId));

                if (conversation == null)
                    throw new ApplicationException("Conversation not found");

                var messages = await _context.Messages
                    .Where(m => m.ConversationId == conversationId)
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => MapToMessageResponse(m, conversation))
                    .ToListAsync();

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation: {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<MessageResponse> SendMessageAsync(SendMessageRequest request, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Page)
                    .FirstOrDefaultAsync(c => c.Id == request.ConversationId && 
                        (c.Bot.UserId == userId || c.Page.UserId == userId));

                if (conversation == null)
                    throw new ApplicationException("Conversation not found");

                var message = new Message
                {
                    Content = request.Content,
                    SenderType = "agent",
                    MessageType = request.MessageType,
                    IsAutoReply = request.IsAutoReply,
                    IsAISuggestion = request.AISuggestions != null && request.AISuggestions.Any(),
                    AISuggestions = request.AISuggestions != null ? 
                        System.Text.Json.JsonSerializer.Serialize(request.AISuggestions) : "[]",
                    ConversationId = conversation.Id,
                    SentByUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsSent = true,
                    IsDelivered = true
                };

                await _context.Messages.AddAsync(message);
                
                // Update conversation
                conversation.LastMessageAt = DateTime.UtcNow;
                conversation.MessageCount++;
                conversation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Message sent to conversation: {ConversationId}", conversation.Id);

                return MapToMessageResponse(message, conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to conversation: {ConversationId}", request.ConversationId);
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId && 
                        (c.Bot.UserId == userId || c.Page.UserId == userId));

                if (conversation == null)
                    return false;

                conversation.IsRead = true;
                conversation.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Conversation marked as read: {ConversationId}", conversationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation as read: {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<bool> AssignConversationAsync(Guid conversationId, Guid assignToUserId, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId && 
                        (c.Bot.UserId == userId || c.Page.UserId == userId));

                if (conversation == null)
                    return false;

                var assignToUser = await _context.Users.FindAsync(assignToUserId);
                if (assignToUser == null)
                    throw new ApplicationException("User not found");

                conversation.AssignedToUserId = assignToUserId;
                conversation.IsAssigned = true;
                conversation.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                // Send notification to assigned user
                await SendAssignmentNotification(assignToUser, conversation);
                
                _logger.LogInformation("Conversation assigned: {ConversationId} -> {UserId}", conversationId, assignToUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning conversation: {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<bool> AddTagAsync(Guid conversationId, string tag, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Tags)
                    .FirstOrDefaultAsync(c => c.Id == conversationId && 
                        (c.Bot.UserId == userId || c.Page.UserId == userId));

                if (conversation == null)
                    return false;

                if (!conversation.Tags.Any(t => t.Tag == tag))
                {
                    conversation.Tags.Add(new ConversationTag
                    {
                        Tag = tag,
                        ConversationId = conversationId,
                        AddedByUserId = userId,
                        AddedAt = DateTime.UtcNow
                    });
                    
                    conversation.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                
                _logger.LogInformation("Tag added to conversation: {ConversationId} -> {Tag}", conversationId, tag);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tag to conversation: {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<bool> RemoveTagAsync(Guid conversationId, string tag, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Tags)
                    .FirstOrDefaultAsync(c => c.Id == conversationId && 
                        (c.Bot.UserId == userId || c.Page.UserId == userId));

                if (conversation == null)
                    return false;

                var tagToRemove = conversation.Tags.FirstOrDefault(t => t.Tag == tag);
                if (tagToRemove != null)
                {
                    conversation.Tags.Remove(tagToRemove);
                    conversation.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                
                _logger.LogInformation("Tag removed from conversation: {ConversationId} -> {Tag}", conversationId, tag);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tag from conversation: {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetAISuggestionsAsync(Guid conversationId)
        {
            try
            {
                // This would integrate with an AI service in production
                // For now, return sample suggestions based on conversation context
                var suggestions = new List<string>
                {
                    "Yes! We have that product in stock. Would you like me to help you place an order?",
                    "Our standard shipping takes 3-5 business days. Express shipping is available for an additional fee.",
                    "We offer a 30-day return policy. All items must be in original condition with tags attached.",
                    "You can track your order using the tracking number sent to your email."
                };

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI suggestions for conversation: {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<bool> ExportConversationAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == conversationId && 
                        (c.Bot.UserId == userId || c.Page.UserId == userId));

                if (conversation == null)
                    return false;

                // In production, this would generate a file and send it via email
                _logger.LogInformation("Conversation exported: {ConversationId}", conversationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting conversation: {ConversationId}", conversationId);
                throw;
            }
        }

        private ConversationResponse MapToConversationResponse(Conversation conversation)
        {
            return new ConversationResponse
            {
                Id = conversation.Id,
                UserName = conversation.UserName,
                Time = GetTimeAgo(conversation.LastMessageAt),
                Platform = conversation.Platform,
                Page = conversation.Page?.Name ?? "Unknown",
                Message = GetLastMessagePreview(conversation),
                Tags = conversation.Tags.Select(t => t.Tag).ToArray(),
                Assigned = conversation.AssignedTo?.UserName,
                AvatarUrl = conversation.AvatarUrl,
                IsRead = conversation.IsRead
            };
        }

        private MessageResponse MapToMessageResponse(Message message, Conversation conversation)
        {
            return new MessageResponse
            {
                Id = message.Id,
                Content = message.Content,
                Sender = message.SenderType,
                Time = message.CreatedAt.ToString("h:mm tt"),
                AvatarUrl = message.SenderType == "user" ? conversation.AvatarUrl : "",
                IsAutoReply = message.IsAutoReply,
                IsAISuggestion = message.IsAISuggestion,
                AISuggestions = message.IsAISuggestion ? 
                    System.Text.Json.JsonSerializer.Deserialize<string[]>(message.AISuggestions) ?? Array.Empty<string>() 
                    : Array.Empty<string>()
            };
        }

        private async Task<ContactInfoResponse> GetContactInfoAsync(Conversation conversation)
        {
            return new ContactInfoResponse
            {
                Name = conversation.UserName,
                Platform = conversation.Platform,
                Page = conversation.Page?.Name ?? "Unknown",
                FirstContact = GetTimeAgo(conversation.CreatedAt),
                Status = conversation.Tags.Any(t => t.Tag == "New Lead") ? "New Lead" : "Active",
                Tags = conversation.Tags.Select(t => t.Tag).ToArray(),
                AssignedTo = conversation.AssignedTo?.UserName
            };
        }

        private string[] GetQuickActions(Conversation conversation)
        {
            var actions = new List<string>
            {
                "Mark as Resolved",
                "Block User",
                "Export Chat"
            };

            if (!conversation.IsAssigned)
                actions.Insert(0, "Assign to Me");

            if (conversation.Priority != "urgent")
                actions.Insert(1, "Mark as Urgent");

            return actions.ToArray();
        }

        private async Task SendAssignmentNotification(User assignToUser, Conversation conversation)
        {
            try
            {
                var emailRequest = new EmailRequest
                {
                    To = assignToUser.Email,
                    Subject = "New Conversation Assigned",
                    Body = $@"
                        <h2>New Conversation Assigned</h2>
                        <p>You have been assigned a new conversation:</p>
                        <ul>
                            <li><strong>User:</strong> {conversation.UserName}</li>
                            <li><strong>Platform:</strong> {conversation.Platform}</li>
                            <li><strong>Page:</strong> {conversation.Page?.Name}</li>
                            <li><strong>Last Message:</strong> {GetLastMessagePreview(conversation)}</li>
                        </ul>
                        <p><a href='{GetDashboardUrl()}/conversations/{conversation.Id}' style='color: #6366F1;'>View Conversation</a></p>
                    ",
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(emailRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending assignment notification");
                // Don't throw, just log the error
            }
        }

        private string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;
            
            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            
            return date.ToString("MMM dd, yyyy");
        }

        private string GetLastMessagePreview(Conversation conversation)
        {
            var lastMessage = _context.Messages
                .Where(m => m.ConversationId == conversation.Id)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            if (lastMessage == null)
                return "No messages";

            var preview = lastMessage.Content.Length > 50 
                ? lastMessage.Content.Substring(0, 47) + "..." 
                : lastMessage.Content;

            return lastMessage.SenderType == "bot" 
                ? $"Bot replied: {preview}" 
                : preview;
        }

        private string GetDashboardUrl()
        {
            // In production, get from configuration
            return "https://app.botflow.com";
        }
    }
}