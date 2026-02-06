using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotFlow.Domain.Entities
{
    public class RecentActivity : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ActivityType { get; set; } = string.Empty; // Conversation, Bot, Page, Report, System
        
        [StringLength(50)]
        public string Icon { get; set; } = string.Empty; // FontAwesome icon class
        
        [StringLength(50)]
        public string Color { get; set; } = "blue"; // blue, green, red, yellow, purple, pink
        
        public string? Metadata { get; set; } // JSON data for additional info
        public Guid? ReferenceId { get; set; } // ID of related entity
        
        public DateTime ActivityTime { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        // Methods
        public string GetIconClass()
        {
            return Icon switch
            {
                "comment" => "fa-comment",
                "robot" => "fa-robot",
                "share" => "fa-share-nodes",
                "chart" => "fa-chart-line",
                "warning" => "fa-triangle-exclamation",
                "check" => "fa-check",
                "plus" => "fa-plus",
                "gear" => "fa-gear",
                "users" => "fa-users",
                _ => "fa-circle-info"
            };
        }
        
        public string GetColorClass()
        {
            return Color switch
            {
                "blue" => "from-blue-500 to-blue-600",
                "green" => "from-green-500 to-green-600",
                "red" => "from-red-500 to-red-600",
                "yellow" => "from-yellow-500 to-yellow-600",
                "purple" => "from-purple-500 to-purple-600",
                "pink" => "from-pink-500 to-pink-600",
                _ => "from-gray-500 to-gray-600"
            };
        }
    }
}