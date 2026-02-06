using System;
using System.ComponentModel.DataAnnotations.Schema;
using BotFlow.Domain.Enums;

namespace BotFlow.Domain.Entities
{
    public class SocialPage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Column(TypeName = "nvarchar(100)")]
        public string PageName { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(100)")]
        public string PageId { get; set; } = string.Empty;
        
        public SocialPlatform Platform { get; set; } = SocialPlatform.Facebook;
        
        [Column(TypeName = "nvarchar(500)")]
        public string AccessToken { get; set; } = string.Empty;
        
        public bool IsConnected { get; set; } = false;
        public bool IsActive { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Owner/User
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
        
        // العلاقات
        public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}