using System;
using System.ComponentModel.DataAnnotations;

namespace BotFlow.Application.Common.DTOs.Pages
{
    // DTOs for Pages Integration Page
    public class PageResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Followers { get; set; } = string.Empty;
        public int Messages { get; set; }
        public string Metric { get; set; } = string.Empty;
        public string MetricLabel { get; set; } = string.Empty;
        public string ConnectionStatus { get; set; } = string.Empty;
        public string Permissions { get; set; } = string.Empty;
        public string LastSynced { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
        public string IconBg { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string StatusTextColor { get; set; } = string.Empty;
        public string StatusDotColor { get; set; } = string.Empty;
    }

    public class ConnectPageRequest
    {
        [Required]
        public string Platform { get; set; } = string.Empty; // 'facebook', 'instagram'

        [Required]
        public string AccessToken { get; set; } = string.Empty;

        public Guid[]? BotIds { get; set; }
    }

    public class UpdatePageRequest
    {
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public Guid[]? BotIds { get; set; }
        public string? WebhookUrl { get; set; }
    }

    public class SyncPageRequest
    {
        [Required]
        public Guid PageId { get; set; }

        public bool ForceSync { get; set; }
    }

    public class ActivityLogResponse
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconBg { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
    }
}