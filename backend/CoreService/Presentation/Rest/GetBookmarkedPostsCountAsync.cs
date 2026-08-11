using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.ValueObjects;
using Shared.Domain.Errors;
using Shared.Presentation.Abstractions;

namespace CoreService.Presentation.Rest;

using Response = Results<Ok<Count>, Forbid<NotAdminError>>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='getBookmarkedPostsCount']/*" />
    public static async Task<Response> GetBookmarkedPostsCountAsync(
        GetBookmarkedPostsCountRequest request,
        [FromServices] GetBookmarkedPostsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetBookmarkedPostsCountQuery
        {
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
