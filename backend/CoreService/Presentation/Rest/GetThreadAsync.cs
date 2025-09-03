using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Presentation.Abstractions;
using SharedKernel.Presentation.Extensions;
using UserService.Domain.Enums;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>
        GetThreadAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] ThreadId threadId,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadQuery
        {
            ThreadId = threadId,
            Role = RoleType.User,
            QueriedBy = userId
        };

        var result = await messageBus.InvokeAsync<GetThreadQueryResult<ThreadDto>>(query, cancellationToken);

        return result.Match<Results<Ok<ThreadDto>, NotFound<ThreadNotFoundError>, Forbid<NonThreadOwnerError>>>(
            threadDto => TypedResults.Ok(threadDto),
            notFound => TypedResults.NotFound(notFound),
            nonThreadOwner => new Forbid<NonThreadOwnerError>(nonThreadOwner)
        );
    }
}