using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotFlow.Domain.Entities
{
    public class ConversationTag
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Column(TypeName = "nvarchar(50)")]
        public string Tag { get; set; } = string.Empty;
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Keys
        public Guid ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; } = null!;
        
        public Guid? AddedByUserId { get; set; }
        public virtual User? AddedBy { get; set; }
    }
}