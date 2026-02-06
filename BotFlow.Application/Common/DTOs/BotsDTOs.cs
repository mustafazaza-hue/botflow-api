using System;
using System.ComponentModel.DataAnnotations;

namespace BotFlow.Application.Common.DTOs.Bots
{
    // DTOs for Bots Management Page
    public class BotResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Chats { get; set; }
        public string Updated { get; set; } = string.Empty;
        public string[] Platforms { get; set; } = Array.Empty<string>();
        public string Icon { get; set; } = string.Empty;
        public string IconBg { get; set; } = string.Empty;
    }

    public class CreateBotRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string FlowConfiguration { get; set; } = "{}";
        public string WelcomeMessage { get; set; } = string.Empty;
        public string FallbackMessage { get; set; } = string.Empty;
        
        public bool IsAutoResponder { get; set; } = true;
        public Guid[] PageIds { get; set; } = Array.Empty<Guid>();
    }

    public class UpdateBotRequest
    {
        [StringLength(200, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? Status { get; set; }
        public string? FlowConfiguration { get; set; }
        public string? WelcomeMessage { get; set; }
        public string? FallbackMessage { get; set; }
        
        public bool? IsAutoResponder { get; set; }
        public Guid[]? PageIds { get; set; }
    }

    public class BotStatsResponse
    {
        public int TotalBots { get; set; }
        public int ActiveBots { get; set; }
        public int ConversationsToday { get; set; }
        public decimal ResponseRate { get; set; }
    }

    public class BotFilterRequest
    {
        public string? Status { get; set; }
        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; } = "updated";
        public string? SortOrder { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}