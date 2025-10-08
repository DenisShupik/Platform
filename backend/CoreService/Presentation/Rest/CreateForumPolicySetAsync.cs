using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

using Response = Ok<PolicyId>;

public static partial class Api
{
    private static async Task<Response> CreateForumPolicySetAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreatePolicyRequestBody body,
        [FromServices] CreatePolicyCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new CreatePolicyCommand
        {
            Type = body.Type,
            Value = body.Value,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}