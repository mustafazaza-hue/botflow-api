using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotFlow.Domain.Entities
{
    public class Workspace
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(500)")]
        public string LogoUrl { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(50)")]
        public string BrandColor { get; set; } = "#6366F1";
        
        [Column(TypeName = "nvarchar(100)")]
        public string Domain { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(50)")]
        public string Timezone { get; set; } = "UTC";
        
        [Column(TypeName = "nvarchar(10)")]
        public string Language { get; set; } = "en";
        
        // Settings
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public bool WeeklyReports { get; set; } = false;
        
        // Subscription
        [Column(TypeName = "nvarchar(50)")]
        public string Plan { get; set; } = "free"; // 'free', 'pro', 'enterprise'
        
        public DateTime? SubscriptionExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Foreign Keys
        public Guid OwnerId { get; set; }
        public virtual User Owner { get; set; } = null!;
        
        // Relations
        public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Bot> Bots { get; set; } = new List<Bot>();
        public virtual ICollection<Page> Pages { get; set; } = new List<Page>();
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}