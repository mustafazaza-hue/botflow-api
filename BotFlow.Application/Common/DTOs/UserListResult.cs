namespace BotFlow.Application.Common.DTOs.SuperAdmin
{
    public class UserListResult
    {
        public List<SuperAdminUserDto> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}