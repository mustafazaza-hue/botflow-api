using System;
using System.ComponentModel.DataAnnotations;

namespace BotFlow.Application.Common.DTOs.Team
{
    // DTOs for Team Management Page
    public class TeamMemberResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string RoleColor { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string StatusDot { get; set; } = string.Empty;
        public string LastActive { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class InviteMemberRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // 'admin', 'agent', 'viewer'

        public string? Message { get; set; }
    }

    public class UpdateMemberRoleRequest
    {
        [Required]
        public Guid MemberId { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;
    }

    public class TeamStatsResponse
    {
        public int TotalMembers { get; set; }
        public int ActiveNow { get; set; }
        public int ActionsToday { get; set; }
        public int PendingInvites { get; set; }
    }

    public class ActivityLogResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
        public string IconBg { get; set; } = string.Empty;
    }
}