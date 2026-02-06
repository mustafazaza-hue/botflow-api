using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotFlow.Domain.Entities
{
    public class Page
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(50)")]
        public string Platform { get; set; } = string.Empty; // 'facebook', 'instagram'
        
        [Column(TypeName = "nvarchar(200)")]
        public string PlatformId { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(500)")]
        public string AccessToken { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(1000)")]
        public string ProfilePictureUrl { get; set; } = string.Empty;
        
        // Statistics
        public int FollowersCount { get; set; }
        public int MessagesCount { get; set; }
        public decimal EngagementRate { get; set; }
        public decimal ResponseRate { get; set; }
        
        // Status
        [Column(TypeName = "nvarchar(50)")]
        public string ConnectionStatus { get; set; } = "connected"; // 'connected', 'disconnected', 'warning'
        
        [Column(TypeName = "nvarchar(50)")]
        public string PermissionsStatus { get; set; } = "granted"; // 'granted', 'limited', 'expired'
        
        public bool IsActive { get; set; } = true;
        public DateTime? LastSyncedAt { get; set; }
        
        // Configuration
        public string WebhookUrl { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string Permissions { get; set; } = "[]"; // JSON array of permissions
        
        // Foreign Keys
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public Guid? BotId { get; set; }
        public virtual Bot? Bot { get; set; }
        
        // Relations
        public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
        public virtual ICollection<AnalyticsData> Analytics { get; set; } = new List<AnalyticsData>();
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? TokenExpiresAt { get; set; }


        
    }
}