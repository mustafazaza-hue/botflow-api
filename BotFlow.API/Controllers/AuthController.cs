using BotFlow.Application.Common.DTOs.Auth;
using BotFlow.Application.Interfaces;
using BotFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;



namespace BotFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var origin = GetOrigin();
                var response = await _authService.RegisterAsync(request, origin);
                
                _logger.LogInformation("New user registered: {Email}", request.Email);
                
                return Ok(new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "Registration successful. Please check your email to verify your account.",
                    Data = response
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Registration failed for {Email}: {Message}", request.Email, ex.Message);
                
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", request.Email);
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during registration. Please try again later."
                });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var response = await _authService.LoginAsync(request, ipAddress);
                
                // Set refresh token in HTTP-only cookie
                SetTokenCookie(response.RefreshToken);
                
                _logger.LogInformation("User logged in: {Email}", request.Email);
                
                return Ok(new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = response
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Login failed for {Email}: {Message}", request.Email, ex.Message);
                
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", request.Email);
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again later."
                });
            }
        }

        [HttpPost("admin/login")]
        [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var response = await _authService.AdminLoginAsync(request, ipAddress);
                
                // Set admin refresh token in HTTP-only cookie
                SetTokenCookie(response.RefreshToken, true);
                
                _logger.LogInformation("Admin logged in: {Email}", request.Email);
                
                return Ok(new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "Admin login successful",
                    Data = response
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Admin login failed for {Email}: {Message}", request.Email, ex.Message);
                
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin login for {Email}", request.Email);
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during admin login. Please try again later."
                });
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Refresh token is required"
                    });
                }

                var ipAddress = GetIpAddress();
                var response = await _authService.RefreshTokenAsync(refreshToken, ipAddress);
                
                // Update refresh token cookie
                SetTokenCookie(response.RefreshToken);
                
                return Ok(new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = response
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Refresh token failed: {Message}", ex.Message);
                
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while refreshing token. Please login again."
                });
            }
        }

        /* Email verification endpoint disabled
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            // Email verification is disabled in this build.
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Email verification is disabled"
            });
        }
        */
        
        [HttpPost("super-admin/login")]
[ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
public async Task<IActionResult> SuperAdminLogin([FromBody] AdminLoginRequest request)
{
    try
    {
        var ipAddress = GetIpAddress();
        var response = await _authService.SuperAdminLoginAsync(request, ipAddress);
        
        // Set admin refresh token in HTTP-only cookie
        SetTokenCookie(response.RefreshToken, true);
        
        _logger.LogInformation("Super Admin logged in: {Email}", request.Email);
        
        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Message = "Super Admin login successful",
            Data = response
        });
    }
    catch (ApplicationException ex)
    {
        _logger.LogWarning("Super Admin login failed for {Email}: {Message}", request.Email, ex.Message);
        
        return Unauthorized(new ApiResponse<object>
        {
            Success = false,
            Message = ex.Message
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during super admin login for {Email}", request.Email);
        
        return StatusCode(500, new ApiResponse<object>
        {
            Success = false,
            Message = "An error occurred during super admin login. Please try again later."
        });
    }
}




        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var origin = GetOrigin();
                await _authService.ForgotPasswordAsync(request, origin);

                // Always return success to prevent email enumeration
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "If your email is registered, you will receive password reset instructions."
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Forgot password failed for {Email}: {Message}", request.Email, ex.Message);

                // Still return success to prevent email enumeration
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "If your email is registered, you will receive password reset instructions."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password for {Email}", request.Email);

                // Still return success to prevent email enumeration
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "If your email is registered, you will receive password reset instructions."
                });
            }
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(request);
                
                if (!result)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid or expired reset token"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Password reset successfully. You can now login with your new password."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", request.Email);
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while resetting password. Please try again."
                });
            }
        }

        /* Resend verification endpoint disabled
        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResendVerification([FromQuery] string email)
        {
            // Email verification is disabled in this build.
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Email verification is disabled"
            });
        }
        */

        [HttpPost("validate-token")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var isValid = await _authService.ValidateTokenAsync(token);
                
                if (!isValid)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Token is valid"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while validating token."
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                var ipAddress = GetIpAddress();
                
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await _authService.LogoutAsync(refreshToken, ipAddress);
                }

                // Clear cookies
                Response.Cookies.Delete("refreshToken");
                
                _logger.LogInformation("User logged out from IP: {IpAddress}", ipAddress);
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Logged out successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during logout."
                });
            }
        }

        // Helper methods
        private void SetTokenCookie(string token, bool isAdmin = false)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict,
                Secure = true // Set to true in production with HTTPS
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
            
            if (isAdmin)
            {
                Response.Cookies.Append("isAdmin", "true", cookieOptions);
            }
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() 
                ?? "Unknown";
        }

        private string GetOrigin()
        {
            return $"{Request.Scheme}://{Request.Host}";
        }
    }

    // API Response wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}