using System;

namespace BotFlow.Application.Common.DTOs.Auth
{
    public class AuthResponse
    {
        // أضف هذه الخصائص المفقودة
        public Guid UserId { get; set; } // أضف هذا
        public string UserName { get; set; } = string.Empty; // أضف هذا
        public int ExpiresIn { get; set; } // أضف هذا
        public bool RequiresEmailVerification { get; set; } // أضف هذا
        
        // الخصائص الأصلية
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenExpires { get; set; }
        public DateTime RefreshTokenExpires { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}