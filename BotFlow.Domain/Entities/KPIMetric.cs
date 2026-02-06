using System;
using System.ComponentModel.DataAnnotations;

namespace BotFlow.Domain.Entities
{
    public class KPIMetric
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string MetricType { get; set; } = string.Empty; // "UserGrowth", "Revenue", "ResponseTime", "ApiUsage"

        [Required]
        [MaxLength(20)]
        public string Period { get; set; } = string.Empty; // "Daily", "Weekly", "Monthly"

        [Required]
        public DateTime Date { get; set; }

        public double Value { get; set; }
        public double TargetValue { get; set; }
        public double ChangePercentage { get; set; }

        [MaxLength(50)]
        public string Platform { get; set; } = string.Empty; // "UX Pilot", "WhatsApp", "Facebook", "Instagram"

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Constructor
        public KPIMetric()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}