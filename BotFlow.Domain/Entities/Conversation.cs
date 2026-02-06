using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotFlow.Domain.Entities
{
    public class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // User Information
        [Column(TypeName = "nvarchar(200)")]
        public string UserName { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(200)")]
        public string PlatformId { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(50)")]
        public string Platform { get; set; } = string.Empty; // 'facebook', 'instagram'
        
        [Column(TypeName = "nvarchar(500)")]
        public string AvatarUrl { get; set; } = string.Empty;

                [Column(TypeName = "nvarchar(100)")]
        public string CustomerName { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(100)")]
        public string CustomerId { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(2000)")]
        public string Message { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(2000)")]
        public string BotResponse { get; set; } = string.Empty;
        
        // Conversation Details
        [Column(TypeName = "nvarchar(50)")]
        public string Status { get; set; } = "active"; // 'active', 'resolved', 'archived'
        
        [Column(TypeName = "nvarchar(50)")]
        public string Priority { get; set; } = "medium"; // 'low', 'medium', 'high', 'urgent'
        
        public bool IsRead { get; set; } = false;
        public bool IsAssigned { get; set; } = false;
        
        // Statistics
        public int MessageCount { get; set; }
        public int ResponseTime { get; set; } // in seconds
        public DateTime LastMessageAt { get; set; }
        
        // Foreign Keys
        public Guid? BotId { get; set; }
        public virtual Bot? Bot { get; set; }
        
        public Guid? PageId { get; set; }
        public virtual Page? Page { get; set; }
        
        // Owner/User
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
        
        public Guid? AssignedToUserId { get; set; }
        public virtual User? AssignedTo { get; set; }
        
        // Relations
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<ConversationTag> Tags { get; set; } = new List<ConversationTag>();
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}