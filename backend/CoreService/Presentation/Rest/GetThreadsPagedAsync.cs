using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;
using Shared.Presentation.Abstractions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    /// <summary>
    /// Получить постраничный список тем
    /// </summary>
    public static async Task<Results<Ok<List<ThreadDto>>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>
        GetThreadsPagedAsync(
            GetThreadsPagedRequest request,
            [FromServices] GetThreadsPagedQueryHandler<ThreadDto> handler,
            CancellationToken cancellationToken
        )
    {
        var query = new GetThreadsPagedQuery<ThreadDto>
        {
            CreatedBy = request.CreatedBy,
            Status = request.Status,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}