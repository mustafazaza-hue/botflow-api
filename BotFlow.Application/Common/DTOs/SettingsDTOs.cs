using System;
using System.ComponentModel.DataAnnotations;

namespace BotFlow.Application.Common.DTOs.Settings
{
    // DTOs for Settings Page
    public class WorkspaceSettingsRequest
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(50)]
        public string? BrandColor { get; set; }

        [StringLength(100)]
        public string? Domain { get; set; }

        [StringLength(50)]
        public string? Timezone { get; set; }

        [StringLength(10)]
        public string? Language { get; set; }

        public bool? EmailNotifications { get; set; }
        public bool? PushNotifications { get; set; }
        public bool? WeeklyReports { get; set; }
    }

    public class WorkspaceSettingsResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string BrandColor { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool WeeklyReports { get; set; }
        public string Plan { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UploadLogoRequest
    {
        [Required]
        public string Base64Image { get; set; } = string.Empty;

        [StringLength(50)]
        public string FileName { get; set; } = string.Empty;
    }

    public class NotificationSettingsRequest
    {
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool WeeklyReports { get; set; }
    }
}