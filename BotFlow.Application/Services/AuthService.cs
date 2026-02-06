using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BotFlow.Application.Common.DTOs.Auth;
using BotFlow.Application.Interfaces;
using BotFlow.Domain.Entities;
using BotFlow.Domain.Enums;
using BotFlow.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace BotFlow.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string origin)
        {
            // التحقق من وجود البريد الإلكتروني مسبقاً
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new ApplicationException("Email is already registered");

            // التحقق من اسم المستخدم
            if (await _context.Users.AnyAsync(u => u.UserName == request.UserName))
                throw new ApplicationException("Username is already taken");

            // التحقق من تطابق كلمات المرور
            if (request.Password != request.ConfirmPassword)
                throw new ApplicationException("Passwords do not match");

            // إنشاء هاش كلمة المرور
            CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

            // Email verification disabled: do not generate verification token
            // var verificationToken = GenerateVerificationToken();

            // إنشاء المستخدم
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                CompanyName = request.CompanyName,
                PhoneNumber = request.PhoneNumber,
                UserName = request.UserName,
                PasswordHash = Convert.ToBase64String(passwordHash),
                PasswordSalt = Convert.ToBase64String(passwordSalt),
                Role = UserRole.User,
                SubscriptionPlan = "Free",
                IsActive = true,
                // Mark email as verified since verification is disabled
                IsEmailVerified = true,
                IsPhoneVerified = false,
                // EmailVerificationToken = verificationToken,
                // EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // حفظ المستخدم في قاعدة البيانات
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Email verification sending disabled
            // await SendVerificationEmail(user, origin);

            // إنشاء JWT token
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // حفظ refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Email}", user.Email);

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60"),
                IsEmailVerified = user.IsEmailVerified,
                RequiresEmailVerification = !user.IsEmailVerified
            };
        }


        public async Task<AuthResponse> SuperAdminLoginAsync(AdminLoginRequest request, string ipAddress)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == request.Email || u.UserName == request.Email);

    if (user == null)
        throw new ApplicationException("Invalid email or password");

    if (!user.IsActive)
        throw new ApplicationException("Account is deactivated");

    // ⭐⭐ **هنا التعديل المهم**: تحقق من Super Admin فقط
    if (user.Role != UserRole.SuperAdmin)  // فقط SuperAdmin
        throw new ApplicationException("Access denied. Super Admin privileges required");

    // التحقق من كلمة المرور
    if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
    {
        user.IncrementFailedLoginAttempts();
        await _context.SaveChangesAsync();
        throw new ApplicationException("Invalid email or password");
    }

    // التحقق من البريد الإلكتروني
    if (!user.IsEmailVerified)
        throw new ApplicationException("Please verify your email before logging in");

    // تحديث آخر تسجيل دخول
    user.UpdateLastLogin();
    await _context.SaveChangesAsync();

    // إنشاء tokens
    var token = GenerateJwtToken(user);
    var refreshToken = GenerateRefreshToken();

    // تحديث refresh token
    user.RefreshToken = refreshToken;
    user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Super Admin logged in: {Email} from IP: {IpAddress}", user.Email, ipAddress);

    return new AuthResponse
    {
        UserId = user.Id,
        Email = user.Email,
        UserName = user.UserName,
        FullName = user.FullName,
        Role = user.Role.ToString(),
        Token = token,
        RefreshToken = refreshToken,
        ExpiresIn = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60"),
        IsEmailVerified = user.IsEmailVerified
    };
}

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.UserName == request.Email);

            if (user == null)
                throw new ApplicationException("Invalid email or password");

            if (!user.IsActive)
                throw new ApplicationException("Account is deactivated");

            if (user.IsLockedOut)
                throw new ApplicationException("Account is locked. Please try again later");

            // التحقق من كلمة المرور
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                user.IncrementFailedLoginAttempts();
                await _context.SaveChangesAsync();
                throw new ApplicationException("Invalid email or password");
            }

            // التحقق من البريد الإلكتروني
            if (!user.IsEmailVerified)
                throw new ApplicationException("Please verify your email before logging in");

            // تحديث آخر تسجيل دخول
            user.UpdateLastLogin();
            await _context.SaveChangesAsync();

            // إنشاء tokens
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // تحديث refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged in: {Email} from IP: {IpAddress}", user.Email, ipAddress);

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60"),
                IsEmailVerified = user.IsEmailVerified
            };
        }

        public async Task<AuthResponse> AdminLoginAsync(AdminLoginRequest request, string ipAddress)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.UserName == request.Email);

            if (user == null)
                throw new ApplicationException("Invalid email or password");

            if (!user.IsActive)
                throw new ApplicationException("Account is deactivated");

            // التحقق من الصلاحيات
            if (user.Role != UserRole.Admin && user.Role != UserRole.SuperAdmin)
                throw new ApplicationException("Access denied. Admin privileges required");

            // التحقق من كلمة المرور
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                user.IncrementFailedLoginAttempts();
                await _context.SaveChangesAsync();
                throw new ApplicationException("Invalid email or password");
            }

            // التحقق من البريد الإلكتروني
            if (!user.IsEmailVerified)
                throw new ApplicationException("Please verify your email before logging in");

            // تحديث آخر تسجيل دخول
            user.UpdateLastLogin();
            await _context.SaveChangesAsync();

            // إنشاء tokens
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // تحديث refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin logged in: {Email} from IP: {IpAddress}", user.Email, ipAddress);

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60"),
                IsEmailVerified = user.IsEmailVerified
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == token);

            if (user == null)
                throw new ApplicationException("Invalid refresh token");

            if (user.RefreshTokenExpires < DateTime.UtcNow)
                throw new ApplicationException("Refresh token has expired");

            if (!user.IsActive)
                throw new ApplicationException("Account is deactivated");

            // إنشاء tokens جديدة
            var newJwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // تحديث refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Token = newJwtToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60"),
                IsEmailVerified = user.IsEmailVerified
            };
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            // Email verification feature is disabled; consider token valid
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null || !user.IsEmailVerified)
                return false; // لا تكشف أن المستخدم غير موجود

            // إنشاء رمز إعادة تعيين كلمة المرور
            var resetToken = GenerateResetToken();
            user.SetPasswordResetToken(resetToken);

            await _context.SaveChangesAsync();

            // إرسال إيميل إعادة تعيين كلمة المرور
            await SendPasswordResetEmail(user, resetToken, origin);

            _logger.LogInformation("Password reset requested for user: {Email}", user.Email);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

            if (user == null)
                return false;

            if (user.PasswordResetTokenExpires < DateTime.UtcNow)
                return false;

            if (request.NewPassword != request.ConfirmPassword)
                return false;

            // تحديث كلمة المرور
            CreatePasswordHash(request.NewPassword, out var passwordHash, out var passwordSalt);
            user.ChangePassword(
                Convert.ToBase64String(passwordHash),
                Convert.ToBase64String(passwordSalt)
            );

            // مسح token إعادة التعيين
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _context.SaveChangesAsync();

            // إرسال إيميل تأكيد تغيير كلمة المرور
            await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName);

            _logger.LogInformation("Password reset successful for user: {Email}", user.Email);

            return true;
        }

        public async Task<bool> ResendVerificationEmailAsync(string email, string origin)
        {
            // Email verification is disabled; nothing to resend
            return true;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] 
                    ?? "BotFlowSuperSecretKey@2024!ChangeThisInProduction123456");

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "BotFlow.API",
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"] ?? "BotFlow.Client",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LogoutAsync(string token, string ipAddress)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == token);

            if (user == null)
                return false;

            // مسح refresh token
            user.ClearRefreshToken();
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged out: {Email} from IP: {IpAddress}", user.Email, ipAddress);

            return true;
        }

        // ========== Private Helper Methods ==========

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] 
                ?? "BotFlowSuperSecretKey@2024!ChangeThisInProduction123456");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("FullName", user.FullName),
                    new Claim("Company", user.CompanyName),
                    new Claim("IsEmailVerified", user.IsEmailVerified.ToString()),
                    new Claim("SubscriptionPlan", user.SubscriptionPlan)
                }),
                Expires = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60")),
                Issuer = _configuration["Jwt:Issuer"] ?? "BotFlow.API",
                Audience = _configuration["Jwt:Audience"] ?? "BotFlow.Client",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateVerificationToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string GenerateResetToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return false;

            var hashBytes = Convert.FromBase64String(storedHash);
            var saltBytes = Convert.FromBase64String(storedSalt);

            using var hmac = new HMACSHA512(saltBytes);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            
            return computedHash.SequenceEqual(hashBytes);
        }

        private async Task SendVerificationEmail(User user, string origin, string? customToken = null)
        {
            // Email verification sending is disabled.
            _logger.LogInformation("Email verification disabled — skipping send for {Email}", user.Email);
            await Task.CompletedTask;
        }

        private async Task SendPasswordResetEmail(User user, string resetToken, string origin)
        {
            var resetUrl = $"{origin}/reset-password?token={resetToken}";

            await _emailService.SendPasswordResetAsync(
                user.Email,
                user.FullName,
                resetToken
            );
        }
    }
}