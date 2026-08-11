using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;

namespace CoreService.Presentation.Rest;

using Response = Ok<IReadOnlyList<ForumDto>>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='getForumsPaged']/*" />
    public static async Task<Response> GetForumsPagedAsync(
        GetForumsPagedRequest request,
        [FromServices] GetForumsPagedQueryHandler<ForumDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsPagedQuery<ForumDto>
        {
            Title = request.Title,
            CreatedBy = request.CreatedBy,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
