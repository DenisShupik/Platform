using System.Security.Claims;

namespace Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim == null) throw new Exception("Sub is not found");
        return Guid.Parse(claim);
    }
}