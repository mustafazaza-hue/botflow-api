using System.ComponentModel.DataAnnotations;

namespace BotFlow.Application.Common.DTOs.Auth
{


    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }
}