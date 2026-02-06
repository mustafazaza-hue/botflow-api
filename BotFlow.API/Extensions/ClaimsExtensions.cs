using System.Security.Claims;

namespace BotFlow.API.Extensions
{
    public static class ClaimsExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? user.FindFirst("uid")?.Value
                           ?? user.FindFirst("UserId")?.Value;
            
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            
            throw new UnauthorizedAccessException("User ID not found in claims.");
        }

        public static string GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value 
                ?? user.FindFirst("email")?.Value 
                ?? throw new UnauthorizedAccessException("Email not found in claims.");
        }

        public static string GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value 
                ?? user.FindFirst("role")?.Value 
                ?? "User";
        }
    }
}