using BotFlow.Application.Common.DTOs.Auth;

namespace BotFlow.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, string origin);
        Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress);
        Task<AuthResponse> AdminLoginAsync(AdminLoginRequest request, string ipAddress);
        Task<AuthResponse> SuperAdminLoginAsync(AdminLoginRequest request, string ipAddress);
        Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<bool> ResendVerificationEmailAsync(string email, string origin);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> LogoutAsync(string token, string ipAddress);
    }
}