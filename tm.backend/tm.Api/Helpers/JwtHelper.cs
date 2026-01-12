using System.Security.Claims;

namespace tm.Api.Helpers;

public static class JwtHelper
{
    public static Guid GetUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst("UserId")?.Value 
            ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        
        throw new UnauthorizedAccessException("Invalid user token");
    }
}

