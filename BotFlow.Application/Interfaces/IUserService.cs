using BotFlow.Application.Common.DTOs.Users;
using BotFlow.Application.Common.DTOs.SuperAdmin;
using DashboardOverviewDto = BotFlow.Application.Common.DTOs.SuperAdmin.DashboardOverviewDto;
using RecentUserDto = BotFlow.Application.Common.DTOs.SuperAdmin.RecentUserDto;
using SubscriptionDistributionDto = BotFlow.Application.Common.DTOs.SuperAdmin.SubscriptionDistributionDto;
using UserStatsDto = BotFlow.Application.Common.DTOs.SuperAdmin.UserStatsDto;

namespace BotFlow.Application.Interfaces
{
    public interface IUserService
    {
        // ==================== Super Admin Methods ====================
        Task<DashboardOverviewDto> GetDashboardOverviewAsync();
        Task<List<RecentUserDto>> GetRecentUsersAsync(int page = 1, int pageSize = 10);
        Task<SubscriptionDistributionDto> GetSubscriptionDistributionAsync();
        Task<UserStatsDto> GetUserStatsAsync();
        Task<UserListResult> GetAllUsersAsync(int page, int pageSize, string search, string role, string status);
        Task<Domain.Entities.User?> GetUserByIdAsync(Guid id);
        Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task<bool> SuspendUserAsync(Guid id, string reason);
        Task<bool> ActivateUserAsync(Guid id);

        // ==================== Existing User Methods ====================
        Task<UserDto> GetByIdAsync(Guid id);
        Task<UserDto> GetByEmailAsync(string email);
        Task<UserDto> GetByUsernameAsync(string username);
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto> CreateAsync(CreateUserRequest request);
        Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request);
        Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ToggleActiveAsync(Guid id);
        Task<UserStatisticsDto> GetUserStatisticsAsync(Guid userId);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<bool> CheckUsernameAvailabilityAsync(string username);
        Task<bool> CheckEmailAvailabilityAsync(string email);
    }
}