using BotFlow.Application.Common.DTOs.Pages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotFlow.Application.Services
{
    public interface IPagesService
    {
        Task<IEnumerable<PageResponse>> GetPagesAsync(Guid userId);
        Task<PageResponse> GetPageByIdAsync(Guid id);
        Task<PageResponse> ConnectPageAsync(ConnectPageRequest request, Guid userId);
        Task<PageResponse> UpdatePageAsync(Guid id, UpdatePageRequest request);
        Task<bool> DisconnectPageAsync(Guid id);
        Task<bool> SyncPageAsync(SyncPageRequest request);
        Task<bool> ReconnectPageAsync(Guid id);
        Task<IEnumerable<ActivityLogResponse>> GetActivityLogsAsync(Guid userId);
        Task<object> GetConnectionStatusAsync(Guid userId);
    }
}