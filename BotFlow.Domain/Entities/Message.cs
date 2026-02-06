using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotFlow.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // change nvarchar(max) to TEXT for SQLite
        [Column(TypeName = "TEXT")]
        public string Content { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(20)")]
        public string SenderType { get; set; } = "user"; // 'user', 'bot', 'agent'
        
        [Column(TypeName = "nvarchar(50)")]
        public string MessageType { get; set; } = "text"; // 'text', 'image', 'quick_reply', 'button'
        
        public string Metadata { get; set; } = "{}"; // JSON metadata
        
        // AI Suggestions
        public bool IsAutoReply { get; set; }
        public bool IsAISuggestion { get; set; }
        public string AISuggestions { get; set; } = "[]"; // JSON array of suggestions
        
        // Status
        public bool IsDelivered { get; set; } = true;
        public bool IsRead { get; set; }
        public bool IsSent { get; set; } = true;
        
        // Foreign Keys
        public Guid ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; } = null!;
        
        public Guid? SentByUserId { get; set; }
        public virtual User? SentBy { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
    }
}