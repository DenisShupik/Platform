using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;
using Shared.Domain.Errors;
using Shared.Presentation.Abstractions;

namespace CoreService.Presentation.Rest;

using Response = Results<Ok<IReadOnlyList<PostDto>>, Forbid<NotAdminError>>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='getBookmarkedPostsPaged']/*" />
    public static async Task<Response> GetBookmarkedPostsPagedAsync(
        GetBookmarkedPostsPagedRequest request,
        [FromServices] GetBookmarkedPostsPagedQueryHandler<PostDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetBookmarkedPostsPagedQuery<PostDto>
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            UserId = request.UserId,
            RequestedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);
        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => new Forbid<NotAdminError>(error)
        );
    }
}
