namespace BotFlow.Application.Common.DTOs.SuperAdmin
{
    public class SuperAdminUserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public string SubscriptionPlan { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? PhoneVerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ActivePagesCount { get; set; }
        public int ActiveBotsCount { get; set; }
        public int TotalConversations { get; set; }
        public int TodayConversations { get; set; }
        public int ConnectedPages { get; set; }
        public int ActiveBots { get; set; }
        public int TotalMessages { get; set; }
        public string MonthlyRevenue { get; set; } = string.Empty;
        public DateTime RenewalDate { get; set; }
        public DateTime JoinedDate { get; set; }
        public DateTime LastActive { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public List<SocialPlatformDto> Platforms { get; set; } = new();
    }

    public class SocialPlatformDto
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int PageCount { get; set; }
    }
}