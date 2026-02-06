using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotFlow.Domain.Entities
{
    public class Bot
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(1000)")]
        public string Description { get; set; } = string.Empty;

                [Column(TypeName = "nvarchar(50)")]
        public string Color { get; set; } = "blue";
        
        [Column(TypeName = "nvarchar(50)")]
        public string Type { get; set; } = "Support";
        
        [Column(TypeName = "nvarchar(50)")]
        public string Status { get; set; } = "draft"; // 'active', 'draft', 'paused'
        
        // Statistics
        public int TotalConversations { get; set; }
        public decimal ConversionRate { get; set; }
        public int ActiveUsers { get; set; }
        
        // Configuration
        public string FlowConfiguration { get; set; } = "{}"; // JSON configuration
        public string WelcomeMessage { get; set; } = string.Empty;
        public string FallbackMessage { get; set; } = string.Empty;
        
        // Settings
        public bool IsAutoResponder { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public bool IsPublic { get; set; } = true;
        
        // Foreign Keys
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        // Relations
        public virtual ICollection<Page> Pages { get; set; } = new List<Page>();
        public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
        public virtual ICollection<AnalyticsData> Analytics { get; set; } = new List<AnalyticsData>();
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActivityAt { get; set; }

        
    }
}