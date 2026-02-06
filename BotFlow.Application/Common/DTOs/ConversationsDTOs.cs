using System;
using System.ComponentModel.DataAnnotations;

namespace BotFlow.Application.Common.DTOs.Conversations
{
    // DTOs for Conversations Page
    public class ConversationResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Page { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string? Assigned { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsRead { get; set; }
    }

    public class MessageResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty; // 'user', 'bot', 'agent'
        public string Time { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsAutoReply { get; set; }
        public bool IsAISuggestion { get; set; }
        public string[] AISuggestions { get; set; } = Array.Empty<string>();
    }

    public class SendMessageRequest
    {
        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        public string MessageType { get; set; } = "text";
        public bool IsAutoReply { get; set; }
        public string[]? AISuggestions { get; set; }
    }

    public class ConversationFilterRequest
    {
        public string? Status { get; set; } // 'all', 'unread', 'assigned'
        public string? Platform { get; set; }
        public Guid? PageId { get; set; }
        public Guid? AssignedTo { get; set; }
        public string[]? Tags { get; set; }
        public string? SearchQuery { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ConversationDetailResponse
    {
        public ConversationResponse Conversation { get; set; } = new();
        public MessageResponse[] Messages { get; set; } = Array.Empty<MessageResponse>();
        public ContactInfoResponse ContactInfo { get; set; } = new();
        public string[] QuickActions { get; set; } = Array.Empty<string>();
    }

    public class ContactInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Page { get; set; } = string.Empty;
        public string FirstContact { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string? AssignedTo { get; set; }
    }
}