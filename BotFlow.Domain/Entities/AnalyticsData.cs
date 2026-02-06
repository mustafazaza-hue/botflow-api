using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotFlow.Domain.Entities
{
    public class AnalyticsData
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string MetricType { get; set; } = string.Empty; // 'messages', 'engagement', 'conversion', 'response'
        public string MetricName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;
        
        // Foreign Keys
        public Guid? BotId { get; set; }
        public virtual Bot? Bot { get; set; }
        
        public Guid? PageId { get; set; }
        public virtual Page? Page { get; set; }
        
        public Guid? UserId { get; set; }
        public virtual User? User { get; set; }
        
        // Additional metadata
        public string Platform { get; set; } = string.Empty; // 'facebook', 'instagram', 'whatsapp'
        public string Period { get; set; } = "daily"; // 'daily', 'weekly', 'monthly'
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}