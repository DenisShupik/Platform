using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions.Results;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

using Response =
    Ok<Dictionary<ForumId, Result<ForumDto, ForumNotFoundError, PolicyViolationError, ReadPolicyRestrictedError>>>;

public static partial class Api
{
    private static async Task<Response> GetForumsBulkAsync(
        ClaimsPrincipal claimsPrincipal,
        GetForumsBulkRequest request,
        [FromServices] GetForumsBulkQueryHandler<ForumDto> handler,
        CancellationToken cancellationToken
    )
    {
        var queriedBy = claimsPrincipal.GetUserIdOrNull();
        var query = new GetForumsBulkQuery<ForumDto>
        {
            ForumIds = request.ForumIds,
            QueriedBy = queriedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}