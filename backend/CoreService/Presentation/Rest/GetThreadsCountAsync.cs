using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.ValueObjects;

namespace CoreService.Presentation.Rest;

using Response = Ok<Count>;

public static partial class Api
{
    /// <summary>
    /// Получить количество тем
    /// </summary>
    public static async Task<Response>
        GetThreadsCountAsync(
            GetThreadsCountRequest request,
            [FromServices] GetThreadsCountQueryHandler handler,
            CancellationToken cancellationToken
        )
    {
        var query = new GetThreadsCountQuery
        {
            CreatedBy = request.CreatedBy,
            State = request.Status,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}