using BotFlow.Application.Common.DTOs.Settings;
using System;
using System.Threading.Tasks;

namespace BotFlow.Application.Services
{
    public interface ISettingsService
    {
        Task<WorkspaceSettingsResponse> GetWorkspaceSettingsAsync(Guid userId);
        Task<WorkspaceSettingsResponse> UpdateWorkspaceSettingsAsync(WorkspaceSettingsRequest request, Guid userId);
        Task<object> UploadLogoAsync(UploadLogoRequest request, Guid userId);
        Task<bool> RemoveLogoAsync(Guid userId);
        Task<NotificationSettingsResponse> GetNotificationSettingsAsync(Guid userId);
        Task<NotificationSettingsResponse> UpdateNotificationSettingsAsync(NotificationSettingsRequest request, Guid userId);
        Task<object> GetSubscriptionInfoAsync(Guid userId);
        Task<bool> DeleteWorkspaceAsync(Guid userId, string confirmation);
    }
}