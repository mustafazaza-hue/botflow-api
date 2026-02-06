namespace BotFlow.Application.Common.DTOs.SuperAdmin
{
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
}