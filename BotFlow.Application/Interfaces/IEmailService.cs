using System.Net.Mail;

namespace BotFlow.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailAsync(EmailRequest request);
        Task SendEmailVerificationAsync(string email, string fullName, string token);
        Task SendPasswordResetAsync(string email, string fullName, string token);
        Task SendPasswordChangedNotificationAsync(string email, string fullName);
        Task SendWelcomeEmailAsync(string email, string fullName);
    }

    public class EmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public MailPriority Priority { get; set; } = MailPriority.Normal;
        public List<string>? Attachments { get; set; }
    }
}