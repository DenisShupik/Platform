using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Shared.Presentation.Extensions;

public static class TokenValidatedContextExtensions
{
    public static void TransformRoles(this TokenValidatedContext context, string audience)
    {
        if (context.Principal?.Identity is not ClaimsIdentity identity) return;
        var resourceAccessValue = identity.FindFirst("resource_access")?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccessValue)) return;

        using var resourceAccess = JsonDocument.Parse(resourceAccessValue);
        var clientRoles = resourceAccess
            .RootElement
            .GetProperty(audience)
            .GetProperty("roles");

        foreach (var role in clientRoles.EnumerateArray())
        {
            var value = role.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, value));
            }
        }
    }
}