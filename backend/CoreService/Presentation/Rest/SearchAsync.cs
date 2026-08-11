using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<SearchResultsDto>,
    BadRequest<InvalidSearchCursorError>,
    BadRequest<InvalidSearchPaginationError>>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='search']/*" />
    public static async Task<Response> SearchAsync(
        SearchRequest request,
        [FromServices] SearchQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new SearchQuery
        {
            Term = request.Term,
            Type = request.Type,
            Offset = request.Offset,
            Sort = request.Sort,
            Limit = PaginationLimit.From(request.Limit.Value),
            Cursor = request.Cursor,
            QueriedBy = request.RequestedBy
        }, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            cursorError => TypedResults.BadRequest(cursorError),
            paginationError => TypedResults.BadRequest(paginationError));
    }
}
