using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<List<ThreadDto>>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>
        GetThreadsPagedAsync(
            ClaimsPrincipal claimsPrincipal,
            GetThreadsPagedRequest request,
            [FromServices] GetThreadsPagedQueryHandler<ThreadDto> handler,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadsPagedQuery<ThreadDto>
        {
            CreatedBy = request.CreatedBy,
            Status = request.Status,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            QueriedBy = userId
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Results<Ok<List<ThreadDto>>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>(
            threads => TypedResults.Ok(threads),
            notAdmin => new Forbid<NotAdminError>(notAdmin),
            notOwner => new Forbid<NotOwnerError>(notOwner)
        );
    }
}