namespace BotFlow.Application.Common.DTOs.SuperAdmin
{
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
}