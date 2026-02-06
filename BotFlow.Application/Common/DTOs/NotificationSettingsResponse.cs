namespace BotFlow.Application.Common.DTOs.Settings
{
    public class NotificationSettingsResponse
    {
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool WeeklyReports { get; set; }
    }
}