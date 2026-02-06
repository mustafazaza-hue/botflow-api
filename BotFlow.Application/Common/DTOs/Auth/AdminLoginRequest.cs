using System.ComponentModel.DataAnnotations;

namespace BotFlow.Application.Common.DTOs.Auth
{
    public class AdminLoginRequest
    {
        [Required(ErrorMessage = "Admin email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "2FA code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "2FA code must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "2FA code must be 6 digits")]
        public string TwoFactorCode { get; set; } = string.Empty;

        public bool RememberDevice { get; set; } = false;
    }
}