using System;
using System.ComponentModel.DataAnnotations;

namespace BotFlow.Domain.Entities
{
    public class AIDataSource
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // "Document", "URL", "API", "Database"

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // "Active", "Processing", "Failed", "Disabled"

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(50)]
        public string FileType { get; set; } = string.Empty; // "PDF", "TXT", "DOCX", "JSON"

        public long FileSize { get; set; } // في bytes
        public int QueryCount { get; set; }
        public int DocumentCount { get; set; }

        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ApiEndpoint { get; set; } = string.Empty;

        [MaxLength(50)]
        public string DatabaseType { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string ErrorMessage { get; set; } = string.Empty;

        public double ProgressPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastProcessedAt { get; set; }
        public Guid UserId { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;

        // Constructor
        public AIDataSource()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Status = "Processing";
        }

        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsProcessed()
        {
            LastProcessedAt = DateTime.UtcNow;
            Status = "Active";
            ProgressPercentage = 100;
        }

        public void MarkAsFailed(string error)
        {
            Status = "Failed";
            ErrorMessage = error;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}