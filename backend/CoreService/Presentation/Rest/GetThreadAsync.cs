using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Extensions;
using UserService.Domain.Enums;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>
        GetThreadAsync(
            ClaimsPrincipal claimsPrincipal,
            GetThreadRequest  request,
            [FromServices] GetThreadQueryHandler<ThreadDto> handler,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadQuery<ThreadDto>
        {
            ThreadId = request.ThreadId,
            Role = RoleType.User,
            QueriedBy = userId
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>(
            threadDto => TypedResults.Ok(threadDto),
            notFound => TypedResults.NotFound(notFound),
            nonThreadOwner => new Forbid<NonThreadOwnerError>(nonThreadOwner)
        );
    }
}