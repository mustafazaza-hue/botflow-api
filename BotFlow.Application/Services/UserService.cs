using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BotFlow.Application.Common.DTOs.Users;
using BotFlow.Application.Common.DTOs.SuperAdmin;
using BotFlow.Application.Interfaces;
using BotFlow.Domain.Entities;
using BotFlow.Domain.Enums;
using BotFlow.Infrastructure.Data;
using System.Security.Claims;
using DashboardOverviewDto = BotFlow.Application.Common.DTOs.SuperAdmin.DashboardOverviewDto;
using RecentUserDto = BotFlow.Application.Common.DTOs.SuperAdmin.RecentUserDto;
using SubscriptionDistributionDto = BotFlow.Application.Common.DTOs.SuperAdmin.SubscriptionDistributionDto;
using UserStatsDto = BotFlow.Application.Common.DTOs.SuperAdmin.UserStatsDto;
using SuperAdminUserDto = BotFlow.Application.Common.DTOs.SuperAdmin.SuperAdminUserDto;

namespace BotFlow.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailService _emailService;

        public UserService(
            ApplicationDbContext context, 
            ILogger<UserService> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // ==================== Super Admin Methods ====================

        public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeSubscriptions = await _context.Users.CountAsync(u => u.IsActive);
            var trialUsers = await _context.Users.CountAsync(u => u.SubscriptionPlan == "Trial");
            var suspendedUsers = await _context.Users.CountAsync(u => !u.IsActive);
            var activeBots = await _context.Bots.CountAsync(b => b.Status == "Active");
            var totalBots = await _context.Bots.CountAsync();

            return new DashboardOverviewDto
            {
                TotalUsers = totalUsers,
                ActiveSubscriptions = activeSubscriptions,
                TrialUsers = trialUsers,
                SuspendedUsers = suspendedUsers,
                MonthlyRevenue = 124800m,
                ActiveBots = activeBots,
                TotalBots = totalBots,
                TotalDocuments = 247,
                ActiveDataSources = 234,
                ProcessingDataSources = 8,
                FailedDataSources = 5,
                UserGrowthPercentage = 12.5m,
                RevenueGrowthPercentage = 18.7m,
                BotGrowthPercentage = 24.1m
            };
        }

        public async Task<List<RecentUserDto>> GetRecentUsersAsync(int page = 1, int pageSize = 10)
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new RecentUserDto
                {
                    Id = u.Id,
                    Name = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    CompanyName = u.CompanyName,
                    Plan = u.SubscriptionPlan,
                    Status = u.IsActive ? "Active" : "Suspended",
                    BotCount = _context.Bots.Count(b => b.UserId == u.Id),
                    Revenue = "$0/mo",
                    JoinedDate = u.CreatedAt,
                    AvatarUrl = "https://storage.googleapis.com/uxpilot-auth.appspot.com/avatars/avatar-4.jpg"
                })
                .ToListAsync();

            return users;
        }

        public async Task<SubscriptionDistributionDto> GetSubscriptionDistributionAsync()
        {
            var business = await _context.Users.CountAsync(u => u.SubscriptionPlan == "Business");
            var pro = await _context.Users.CountAsync(u => u.SubscriptionPlan == "Pro");
            var starter = await _context.Users.CountAsync(u => u.SubscriptionPlan == "Starter");
            var trial = await _context.Users.CountAsync(u => u.SubscriptionPlan == "Trial");

            return new SubscriptionDistributionDto
            {
                Business = business,
                Pro = pro,
                Starter = starter,
                Trial = trial,
                Total = business + pro + starter + trial
            };
        }

        public async Task<UserStatsDto> GetUserStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var today = DateTime.UtcNow.Date;
            var newUsersToday = await _context.Users.CountAsync(u => u.CreatedAt.Date == today);
            
            var activeRate = totalUsers > 0 ? (decimal)activeUsers / totalUsers * 100 : 0;

            return new UserStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                NewUsersToday = newUsersToday,
                NewUsersThisWeek = await _context.Users.CountAsync(u => u.CreatedAt >= today.AddDays(-7)),
                NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= today.AddDays(-30)),
                AverageSessionDuration = "4m 32s",
                ActiveRate = Math.Round(activeRate, 1),
                ChurnRate = 2.4m
            };
        }

        public async Task<UserListResult> GetAllUsersAsync(int page, int pageSize, string search, string role, string status)
        {
            var query = _context.Users.AsQueryable();

            // Filter by search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => 
                    u.Email.Contains(search) || 
                    u.FirstName.Contains(search) || 
                    u.LastName.Contains(search) ||
                    u.CompanyName.Contains(search));
            }

            // Filter by role
            if (!string.IsNullOrEmpty(role))
            {
                if (Enum.TryParse<UserRole>(role, out var roleEnum))
                {
                    query = query.Where(u => u.Role == roleEnum);
                }
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                var isActive = status.ToLower() == "active";
                query = query.Where(u => u.IsActive == isActive);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new SuperAdminUserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    CompanyName = u.CompanyName,
                    UserName = u.UserName,
                    Role = u.Role.ToString(),
                    ProfileImageUrl = u.ProfileImageUrl ?? string.Empty,
                    SubscriptionPlan = u.SubscriptionPlan,
                    IsActive = u.IsActive,
                    IsEmailVerified = u.IsEmailVerified,
                    IsPhoneVerified = u.IsPhoneVerified,
                    FailedLoginAttempts = u.FailedLoginAttempts,
                    IsLockedOut = u.IsLockedOut,
                    LastLoginAt = u.LastLoginAt,
                    EmailVerifiedAt = u.EmailVerifiedAt,
                    PhoneVerifiedAt = u.PhoneVerifiedAt,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    ActivePagesCount = _context.SocialPages.Count(p => p.UserId == u.Id && p.IsActive),
                    ActiveBotsCount = _context.Bots.Count(b => b.UserId == u.Id && b.Status == "Active"),
                    TotalConversations = _context.Conversations.Count(c => c.UserId == u.Id),
                    TodayConversations = _context.Conversations.Count(c => c.UserId == u.Id && c.CreatedAt.Date == DateTime.UtcNow.Date)
                })
                .ToListAsync();

            return new UserListResult
            {
                Users = users,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<Domain.Entities.User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (request.Role.HasValue)
                user.Role = request.Role.Value;

            if (!string.IsNullOrEmpty(request.SubscriptionPlan))
                user.SubscriptionPlan = request.SubscriptionPlan;

            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SuspendUserAsync(Guid id, string reason)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ActivateUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        // ==================== Existing User Methods ====================

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");
            
            var statistics = await GetUserStatisticsAsync(id);
            
            return MapToDto(user, statistics);
        }

        public async Task<UserDto> GetByEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
                throw new KeyNotFoundException($"User with email {email} not found");
            
            var statistics = await GetUserStatisticsAsync(user.Id);
            
            return MapToDto(user, statistics);
        }

        public async Task<UserDto> GetByUsernameAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);
            
            if (user == null)
                throw new KeyNotFoundException($"User with username {username} not found");
            
            var statistics = await GetUserStatisticsAsync(user.Id);
            
            return MapToDto(user, statistics);
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();
            
            var userDtos = new List<UserDto>();
            
            foreach (var user in users)
            {
                var statistics = await GetUserStatisticsAsync(user.Id);
                userDtos.Add(MapToDto(user, statistics));
            }
            
            return userDtos;
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new ArgumentException($"Email {request.Email} is already registered");
            
            if (await _context.Users.AnyAsync(u => u.UserName == request.UserName))
                throw new ArgumentException($"Username {request.UserName} is already taken");
            
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                CompanyName = request.CompanyName,
                PhoneNumber = request.PhoneNumber,
                UserName = request.UserName,
                PasswordHash = Convert.ToBase64String(passwordHash),
                PasswordSalt = Convert.ToBase64String(passwordSalt),
                Role = request.Role,
                SubscriptionPlan = "Free",
                IsActive = true,
                IsEmailVerified = true,
                IsPhoneVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User {UserId} created successfully", user.Id);
            
            return MapToDto(user);
        }

        public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");
            
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.CompanyName = request.CompanyName ?? user.CompanyName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            
            if (request.Role.HasValue)
                user.Role = request.Role.Value;
            
            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;
            
            if (!string.IsNullOrEmpty(request.ProfileImageUrl))
                user.ProfileImageUrl = request.ProfileImageUrl;
            
            if (!string.IsNullOrEmpty(request.SubscriptionPlan))
                user.SubscriptionPlan = request.SubscriptionPlan;
            
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User {UserId} updated successfully", id);
            
            return MapToDto(user);
        }

        public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");
            
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.CompanyName = request.CompanyName;
            user.PhoneNumber = request.PhoneNumber;
            
            if (!string.IsNullOrEmpty(request.ProfileImageUrl))
                user.ProfileImageUrl = request.ProfileImageUrl;
            
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User {UserId} updated profile", userId);
            
            return MapToDto(user);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");
            
            if (!VerifyPasswordHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                throw new UnauthorizedAccessException("Current password is incorrect");
            
            CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordSalt);
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User {UserId} changed password", userId);
            
            await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName);
            
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return false;
            
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User {UserId} deactivated", id);
            
            return true;
        }

        public async Task<bool> ToggleActiveAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");
            
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User {UserId} active status changed to {IsActive}", id, user.IsActive);
            
            return user.IsActive;
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync(Guid userId)
        {
            var today = DateTime.UtcNow.Date;
            
            var totalPages = await _context.SocialPages
                .CountAsync(p => p.UserId == userId);
            
            var activePages = await _context.SocialPages
                .CountAsync(p => p.UserId == userId && p.IsActive);
            
            var totalBots = await _context.Bots
                .CountAsync(b => b.UserId == userId);
            
            var activeBots = await _context.Bots
                .CountAsync(b => b.UserId == userId && b.Status == BotStatus.Active.ToString());
            
            var totalConversations = await _context.Conversations
                .CountAsync(c => c.UserId == userId);
            
            var todayConversations = await _context.Conversations
                .CountAsync(c => c.UserId == userId && c.CreatedAt.Date == today);
            
            var resolvedConversations = await _context.Conversations
                .CountAsync(c => c.UserId == userId && c.Status == ConversationStatus.Resolved.ToString());
            
            var conversationsWithResponse = await _context.Conversations
                .Where(c => c.UserId == userId && c.ResponseTime != null)
                .ToListAsync();
            
            var averageResponseTime = conversationsWithResponse.Any()
                ? conversationsWithResponse.Average(c => c.ResponseTime)
                : 0;
            
            var responseRate = totalConversations > 0
                ? (double)conversationsWithResponse.Count / totalConversations * 100
                : 0;
            
            return new UserStatisticsDto
            {
                UserId = userId,
                TotalPages = totalPages,
                ActivePages = activePages,
                TotalBots = totalBots,
                ActiveBots = activeBots,
                TotalConversations = totalConversations,
                TodayConversations = todayConversations,
                ResolvedConversations = resolvedConversations,
                AverageResponseTime = Math.Round(averageResponseTime, 2),
                ResponseRate = Math.Round(responseRate, 1)
            };
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            
            if (user == null || !user.IsActive)
                return false;
            
            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(24);
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            await _emailService.SendPasswordResetAsync(user.Email, user.FullName, resetToken);
            
            _logger.LogInformation("Password reset requested for user {UserId}", user.Id);
            
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            
            if (user == null || user.PasswordResetTokenExpires < DateTime.UtcNow)
                return false;
            
            CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordSalt);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName);
            
            _logger.LogInformation("User {UserId} reset password", user.Id);
            
            return true;
        }

        public async Task<bool> CheckUsernameAvailabilityAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.UserName == username);
        }

        public async Task<bool> CheckEmailAvailabilityAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

        // Private Helper Methods
        private UserDto MapToDto(User user, UserStatisticsDto? statistics = null)
        {
            var dto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CompanyName = user.CompanyName,
                UserName = user.UserName,
                Role = user.Role,
                ProfileImageUrl = user.ProfileImageUrl ?? string.Empty,
                SubscriptionPlan = user.SubscriptionPlan,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified,
                FailedLoginAttempts = user.FailedLoginAttempts,
                IsLockedOut = user.IsLockedOut,
                LastLoginAt = user.LastLoginAt,
                EmailVerifiedAt = user.EmailVerifiedAt,
                PhoneVerifiedAt = user.PhoneVerifiedAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
            
            if (statistics != null)
            {
                dto.ActivePagesCount = statistics.ActivePages;
                dto.ActiveBotsCount = statistics.ActiveBots;
                dto.TotalConversations = statistics.TotalConversations;
                dto.TodayConversations = statistics.TodayConversations;
            }
            
            return dto;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return false;

            var hashBytes = Convert.FromBase64String(storedHash);
            var saltBytes = Convert.FromBase64String(storedSalt);

            using var hmac = new System.Security.Cryptography.HMACSHA512(saltBytes);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            
            return computedHash.SequenceEqual(hashBytes);
        }
    }
}