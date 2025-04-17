using System.Security.Claims;
using SharedKernel.Domain.ValueObjects;

namespace SharedKernel.Presentation.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static UserId GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim == null) throw new Exception("Sub is not found");
        return UserId.Parse(claim);
    }
}