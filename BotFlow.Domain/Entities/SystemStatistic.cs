using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotFlow.Domain.Entities
{
    public class SystemStatistic
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public int TotalUsers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TrialUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public int TotalBots { get; set; }
        public int TotalDocuments { get; set; }
        public int ActiveDataSources { get; set; }
        public int ProcessingDataSources { get; set; }
        public int FailedDataSources { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalRevenue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UXPilotApiCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WhatsAppApiCost { get; set; }

        public double AvgResponseTime { get; set; } // في milliseconds
        public double ServerUptime { get; set; } // في نسبة مئوية
        public double DatabaseLoad { get; set; } // في نسبة مئوية
        public double BotSuccessRate { get; set; } // في نسبة مئوية
        public double ErrorRate { get; set; } // في نسبة مئوية
        public int TotalComments { get; set; }
        public int MessagesSent { get; set; }
        public int UXPilotApiCalls { get; set; }
        public int WhatsAppApiCalls { get; set; }

        public DateTime CreatedAt { get; set; }

        // Constructor
        public SystemStatistic()
        {
            Id = Guid.NewGuid();
            Date = DateTime.UtcNow.Date;
            CreatedAt = DateTime.UtcNow;
        }
    }
}