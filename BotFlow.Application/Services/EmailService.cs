using BotFlow.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace BotFlow.Application.Services
{
    public class EmailService : IEmailService, IDisposable
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly SmtpClient? _smtpClient;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _sendRealEmails;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            // الحصول على القيمة كـ bool (يجب استخدام GetValue<bool>)
            var sendRealEmailsString = configuration["EmailSettings:SendRealEmails"];
            _sendRealEmails = bool.TryParse(sendRealEmailsString, out var result) && result;

            if (_sendRealEmails)
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                
                var host = smtpSettings["Host"];
                var portString = smtpSettings["Port"];
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var enableSslString = smtpSettings["EnableSsl"];

                if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(portString) && 
                    !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    _smtpClient = new SmtpClient(host)
                    {
                        Port = int.Parse(portString),
                        Credentials = new NetworkCredential(username, password),
                        EnableSsl = bool.TryParse(enableSslString, out var ssl) && ssl,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Timeout = 10000
                    };
                }
                else
                {
                    _logger.LogWarning("SMTP settings are incomplete. Falling back to console logging.");
                    _sendRealEmails = false;
                }
            }

            _fromEmail = configuration["SmtpSettings:FromEmail"] ?? "noreply@botflow.com";
            _fromName = configuration["SmtpSettings:FromName"] ?? "BotFlow System";
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            await SendEmailAsync(new EmailRequest
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = true
            });
        }

        public async Task SendEmailAsync(EmailRequest request)
        {
            try
            {
                if (_sendRealEmails && _smtpClient != null)
                {
                    await SendRealEmailAsync(request);
                }
                else
                {
                    await LogEmailToConsoleAsync(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", request.To);
                throw new ApplicationException($"Failed to send email: {ex.Message}");
            }
        }

        private async Task SendRealEmailAsync(EmailRequest request)
        {
            if (_smtpClient == null)
            {
                throw new InvalidOperationException("SMTP client is not configured properly.");
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = request.IsHtml,
                Priority = request.Priority,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(request.To);

            if (!string.IsNullOrEmpty(request.Cc))
                mailMessage.CC.Add(request.Cc);

            if (!string.IsNullOrEmpty(request.Bcc))
                mailMessage.Bcc.Add(request.Bcc);

            if (request.Attachments != null && request.Attachments.Any())
            {
                foreach (var attachmentPath in request.Attachments)
                {
                    if (File.Exists(attachmentPath))
                    {
                        mailMessage.Attachments.Add(new Attachment(attachmentPath));
                    }
                }
            }

            await _smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("✅ Email sent successfully to {To}", request.To);
        }

        private async Task LogEmailToConsoleAsync(EmailRequest request)
        {
            var emailPreview = request.Body.Length > 200 
                ? request.Body[..200] + "..." 
                : request.Body;

            _logger.LogInformation("📧 Email Logged (Development Mode):");
            _logger.LogInformation("To: {To}", request.To);
            _logger.LogInformation("Subject: {Subject}", request.Subject);
            _logger.LogInformation("Body Preview: {BodyPreview}", emailPreview);
            _logger.LogInformation("IsHtml: {IsHtml}", request.IsHtml);
            _logger.LogInformation("Priority: {Priority}", request.Priority);

            await Task.Delay(100);
        }

        public async Task SendEmailVerificationAsync(string email, string fullName, string token)
        {
            // Email verification is disabled in this build. Log and skip sending.
            _logger.LogInformation("Email verification disabled — would send verification to {Email}", email);
            await Task.CompletedTask;
        }

        public async Task SendPasswordResetAsync(string email, string fullName, string token)
        {
            var appBaseUrl = _configuration["App:BaseUrl"] ?? "https://botflow.com";
            var resetUrl = $"{appBaseUrl}/reset-password?token={token}";
            
            var body = BuildEmailTemplate(
                "🔑 Reset Your BotFlow Password",
                fullName,
                $@"
                <div style='background: #fff3cd; border: 1px solid #ffeaa7; color: #856404; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <strong>⚠️ Important:</strong> If you didn't request a password reset, please ignore this email or contact our support team immediately.
                </div>
                
                <p>We received a request to reset the password for your BotFlow account associated with this email address.</p>
                
                <p>To reset your password, click the button below:</p>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{resetUrl}' style='display: inline-block; background: linear-gradient(135deg, #6366F1 0%, #8B5CF6 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Reset My Password</a>
                </div>
                
                <p>Or copy and paste this link into your browser:</p>
                <p style='background: #f0f0f0; padding: 10px; border-radius: 5px; word-break: break-all;'>
                    {resetUrl}
                </p>
                
                <p><strong>This reset link will expire in 1 hour for security reasons.</strong></p>
                ",
                email
            );

            await SendEmailAsync(new EmailRequest
            {
                To = email,
                Subject = "🔑 Reset Your BotFlow Password",
                Body = body,
                IsHtml = true,
                Priority = MailPriority.High
            });
        }

        public async Task SendPasswordChangedNotificationAsync(string email, string fullName)
        {
            var body = BuildEmailTemplate(
                "✅ Your BotFlow Password Has Been Changed",
                fullName,
                $@"
                <div style='background: #d1fae5; border: 1px solid #a7f3d0; color: #065f46; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <strong>🔒 Security Notice:</strong>
                    <ul style='margin: 10px 0 0 0; padding-left: 20px;'>
                        <li>If you made this change, no further action is required.</li>
                        <li>If you did NOT change your password, please contact our support team immediately.</li>
                        <li>Ensure your new password is strong and unique.</li>
                    </ul>
                </div>
                
                <p><strong>Date & Time:</strong> {DateTime.UtcNow:MMMM dd, yyyy HH:mm} (UTC)</p>
                <p><strong>Account:</strong> {email}</p>
                
                <p>For security reasons, if you suspect any unauthorized activity on your account:</p>
                <ol>
                    <li>Change your password immediately</li>
                    <li>Review your recent account activity</li>
                    <li>Contact our support team at support@botflow.com</li>
                </ol>
                ",
                email
            );

            await SendEmailAsync(new EmailRequest
            {
                To = email,
                Subject = "✅ Your BotFlow Password Has Been Changed",
                Body = body,
                IsHtml = true,
                Priority = MailPriority.Normal
            });
        }

        public async Task SendWelcomeEmailAsync(string email, string fullName)
        {
            var appBaseUrl = _configuration["App:BaseUrl"] ?? "https://botflow.com";
            
            var body = BuildEmailTemplate(
                "🎉 Welcome to BotFlow - Let's Automate Your Social Media!",
                fullName,
                $@"
                <h3>🚀 Get Started in 3 Easy Steps:</h3>
                
                <div style='background: white; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #6366F1;'>
                    <strong>1. Connect Your First Social Page</strong>
                    <p>Connect your Facebook, Instagram, or WhatsApp business account to start automating conversations.</p>
                </div>
                
                <div style='background: white; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #6366F1;'>
                    <strong>2. Create Your First Bot</strong>
                    <p>Design an AI-powered chatbot to handle customer inquiries 24/7.</p>
                </div>
                
                <div style='background: white; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #6366F1;'>
                    <strong>3. Analyze & Optimize</strong>
                    <p>Use our analytics dashboard to track performance and improve engagement.</p>
                </div>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{appBaseUrl}/dashboard' style='display: inline-block; background: linear-gradient(135deg, #6366F1 0%, #8B5CF6 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Go to Your Dashboard</a>
                </div>
                
                <h3>📚 Helpful Resources:</h3>
                <ul>
                    <li><a href='{appBaseUrl}/docs/getting-started'>Getting Started Guide</a></li>
                    <li><a href='{appBaseUrl}/docs/bot-creation'>How to Create Your First Bot</a></li>
                    <li><a href='{appBaseUrl}/docs/integrations'>Social Media Integrations</a></li>
                    <li><a href='{appBaseUrl}/support'>Live Support & Help Center</a></li>
                </ul>
                ",
                email
            );

            await SendEmailAsync(new EmailRequest
            {
                To = email,
                Subject = "🎉 Welcome to BotFlow - Let's Automate Your Social Media!",
                Body = body,
                IsHtml = true,
                Priority = MailPriority.Normal
            });
        }

        private string BuildEmailTemplate(string title, string recipientName, string content, string recipientEmail)
        {
            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{
            font-family: 'Arial', sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
            background-color: #f5f5f5;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: linear-gradient(135deg, #6366F1 0%, #8B5CF6 100%);
            color: white;
            padding: 30px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }}
        .content {{
            background: #f9f9f9;
            padding: 30px;
            border-radius: 0 0 10px 10px;
            border: 1px solid #e0e0e0;
            border-top: none;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            color: #666666;
            font-size: 12px;
        }}
        a {{
            color: #6366F1;
            text-decoration: none;
        }}
        a:hover {{
            text-decoration: underline;
        }}
        @media only screen and (max-width: 600px) {{
            .container {{
                padding: 10px;
            }}
            .header, .content {{
                padding: 20px;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0; font-size: 24px;'>{title}</h1>
        </div>
        <div class='content'>
            <h2 style='margin-top: 0;'>Hello {recipientName},</h2>
            {content}
            <p style='margin-top: 30px;'>Best regards,<br>The BotFlow Team 🚀</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} BotFlow. All rights reserved.</p>
            <p>This email was sent to {recipientEmail}</p>
            <p>
                <a href='https://botflow.com/unsubscribe'>Unsubscribe</a> | 
                <a href='https://botflow.com/preferences'>Email Preferences</a>
            </p>
        </div>
    </div>
</body>
</html>";
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}