using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotFlow.Domain.Entities
{
    public class TeamMember
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(200)")]
        public string Email { get; set; } = string.Empty;
        
        [Column(TypeName = "nvarchar(50)")]
        public string Role { get; set; } = "viewer"; // 'owner', 'admin', 'agent', 'viewer'
        
        [Column(TypeName = "nvarchar(500)")]
        public string AvatarUrl { get; set; } = string.Empty;
        
        // Status
        [Column(TypeName = "nvarchar(20)")]
        public string Status { get; set; } = "active"; // 'active', 'away', 'offline'
        
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Activity
        public DateTime? LastActiveAt { get; set; }
        public int ActionsToday { get; set; }
        
        // Invitation
        public string InvitationToken { get; set; } = string.Empty;
        public DateTime? InvitationSentAt { get; set; }
        public DateTime? InvitationAcceptedAt { get; set; }
        
        // Foreign Keys
        public Guid WorkspaceId { get; set; }
        public virtual Workspace Workspace { get; set; } = null!;
        
        public Guid? UserId { get; set; }
        public virtual User? User { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}