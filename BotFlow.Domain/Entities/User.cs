using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BotFlow.Domain.Enums;

namespace BotFlow.Domain.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string PasswordSalt { get; set; } = string.Empty;

        // تغيير من string إلى UserRole enum
        public UserRole Role { get; set; } = UserRole.User;

        // إضافة الخصائص المفقودة
        public string SubscriptionPlan { get; set; } = "Free";
        public string? ProfileImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = false;
        public bool IsPhoneVerified { get; set; } = false;

        // Email verification properties (kept for schema compatibility but feature is currently disabled)
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpires { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpires { get; set; }

        public DateTime? LastLoginAt { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? PhoneVerifiedAt { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Social login fields
        public string? GoogleId { get; set; }
        public string? FacebookId { get; set; }
        public string? MicrosoftId { get; set; }

        // Properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;

        // Methods
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            FailedLoginAttempts = 0;
        }

        public void IncrementFailedLoginAttempts()
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= 5)
            {
                LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }
        }

        public void ClearLockout()
        {
            FailedLoginAttempts = 0;
            LockoutEnd = null;
        }

        // Email verification helper commented out (feature disabled)
        // public void SetEmailVerificationToken(string token, int expiryInHours = 24)
        // {
        //     EmailVerificationToken = token;
        //     EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(expiryInHours);
        // }

        public void SetPasswordResetToken(string token, int expiryInHours = 1)
        {
            PasswordResetToken = token;
            PasswordResetTokenExpires = DateTime.UtcNow.AddHours(expiryInHours);
        }

        public void SetRefreshToken(string token, int expiryInDays = 7)
        {
            RefreshToken = token;
            RefreshTokenExpires = DateTime.UtcNow.AddDays(expiryInDays);
        }

        public void ClearRefreshToken()
        {
            RefreshToken = null;
            RefreshTokenExpires = null;
        }

        // Email verification method commented out (feature disabled)
        // public void VerifyEmail()
        // {
        //     IsEmailVerified = true;
        //     EmailVerifiedAt = DateTime.UtcNow;
        //     EmailVerificationToken = null;
        //     EmailVerificationTokenExpires = null;
        // }

        public void VerifyPhone()
        {
            IsPhoneVerified = true;
            PhoneVerifiedAt = DateTime.UtcNow;
        }
        
        // إضافة الدوال المفقودة
        public void UpdateProfile(string firstName, string lastName, string companyName, string phoneNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            CompanyName = companyName;
            PhoneNumber = phoneNumber;
            MarkAsUpdated();
        }
        
        public void ChangePassword(string passwordHash, string passwordSalt)
        {
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            MarkAsUpdated();
        }
    }
}