using System.ComponentModel.DataAnnotations;
using BotFlow.Domain.Enums;

namespace BotFlow.Application.Common.DTOs.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public string SubscriptionPlan { get; set; } = "Free";
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
        
        // الإحصائيات
        public int ActivePagesCount { get; set; }
        public int ActiveBotsCount { get; set; }
        public int TotalConversations { get; set; }
        public int TodayConversations { get; set; }
    }

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

    // ==================== Super Admin DTOs ====================
    public class DashboardOverviewDto
    {
        public int TotalUsers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TrialUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int ActiveBots { get; set; }
        public int TotalBots { get; set; }
        public int TotalDocuments { get; set; }
        public int ActiveDataSources { get; set; }
        public int ProcessingDataSources { get; set; }
        public int FailedDataSources { get; set; }
        public decimal UserGrowthPercentage { get; set; }
        public decimal RevenueGrowthPercentage { get; set; }
        public decimal BotGrowthPercentage { get; set; }
    }

    public class RevenueTrendDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal GrowthPercentage { get; set; }
    }

    public class SubscriptionDistributionDto
    {
        public int Business { get; set; }
        public int Pro { get; set; }
        public int Starter { get; set; }
        public int Trial { get; set; }
        public int Total { get; set; }
    }

    public class RecentUserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int BotCount { get; set; }
        public string Revenue { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class SystemPerformanceDto
    {
        public int ApiResponseTime { get; set; }
        public decimal ServerUptime { get; set; }
        public decimal DatabaseLoad { get; set; }
        public decimal BotSuccessRate { get; set; }
        public decimal ErrorRate { get; set; }
        public int ActiveConnections { get; set; }
        public decimal MemoryUsage { get; set; }
        public decimal CpuUsage { get; set; }
        public decimal DiskUsage { get; set; }
    }

    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public string AverageSessionDuration { get; set; } = string.Empty;
        public decimal ActiveRate { get; set; }
        public decimal ChurnRate { get; set; }
    }

    public class KPIOverviewDto
    {
        public decimal Revenue { get; set; }
        public int Users { get; set; }
        public int ActiveBots { get; set; }
        public int Messages { get; set; }
        public decimal RevenueGrowth { get; set; }
        public decimal UserGrowth { get; set; }
        public decimal BotGrowth { get; set; }
        public decimal MessageGrowth { get; set; }
    }

    public class UserGrowthDto
    {
        public string Period { get; set; } = string.Empty;
        public int Users { get; set; }
        public decimal Growth { get; set; }
    }

    public class ApiUsageDto
    {
        public string Date { get; set; } = string.Empty;
        public int UXPilot { get; set; }
        public int WhatsApp { get; set; }
        public int Total { get; set; }
    }

    public class SystemHealthDto
    {
        public decimal Uptime { get; set; }
        public int ResponseTime { get; set; }
        public decimal ErrorRate { get; set; }
        public int ActiveUsers { get; set; }
        public int ApiCallsPerSecond { get; set; }
        public int DatabaseConnections { get; set; }
        public decimal MemoryUsage { get; set; }
        public decimal CpuUsage { get; set; }
        public string HealthStatus { get; set; } = string.Empty;
    }

    public class CostBreakdownDto
    {
        public decimal UXPilotApiCost { get; set; }
        public decimal WhatsAppApiCost { get; set; }
        public decimal ServerCosts { get; set; }
        public decimal DatabaseCosts { get; set; }
        public decimal ExternalServices { get; set; }
        public decimal SupportCosts { get; set; }
        public decimal MarketingCosts { get; set; }
        public decimal TotalCost { get; set; }
        public decimal NetRevenue { get; set; }
    }

    public class MessageVolumeDto
    {
        public string Date { get; set; } = string.Empty;
        public int Messages { get; set; }
        public int AutoReplies { get; set; }
        public int ManualReplies { get; set; }
    }

    // ==================== Request DTOs ====================
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;
    }

    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? PhoneNumber { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? SubscriptionPlan { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid URL format")]
        public string? ProfileImageUrl { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }

    public class UserStatisticsDto
    {
        public Guid UserId { get; set; }
        public int TotalPages { get; set; }
        public int ActivePages { get; set; }
        public int TotalBots { get; set; }
        public int ActiveBots { get; set; }
        public int TotalConversations { get; set; }
        public int TodayConversations { get; set; }
        public int ResolvedConversations { get; set; }
        public double AverageResponseTime { get; set; }
        public double ResponseRate { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    // Request DTOs for Super Admin operations
    public class UserFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string? Plan { get; set; }
    }

    public class SuperAdminUpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        public string? SubscriptionPlan { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? RenewalDate { get; set; }
        public string? Status { get; set; }
        public string? Email { get; set; }
        public string? CompanyName { get; set; }
    }

    public class SuspendUserRequest
    {
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; } = string.Empty;
    }
}