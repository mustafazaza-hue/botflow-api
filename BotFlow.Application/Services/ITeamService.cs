using BotFlow.Application.Common.DTOs.Team;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotFlow.Application.Services
{
    public interface ITeamService
    {
        Task<IEnumerable<TeamMemberResponse>> GetTeamMembersAsync(Guid userId);
        Task<TeamStatsResponse> GetTeamStatsAsync(Guid userId);
        Task<object> InviteMemberAsync(InviteMemberRequest request, Guid userId);
        Task<bool> UpdateMemberRoleAsync(UpdateMemberRoleRequest request, Guid userId);
        Task<bool> RemoveMemberAsync(Guid memberId, Guid userId);
        Task<IEnumerable<ActivityLogResponse>> GetActivityLogsAsync(Guid userId);
        Task<bool> ResendInviteAsync(Guid memberId, Guid userId);
        Task<IEnumerable<object>> GetPendingInvitesAsync(Guid userId);
    }
}