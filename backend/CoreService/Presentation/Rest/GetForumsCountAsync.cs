using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<ulong>> GetForumsCountAsync(
        GetForumsCountRequest request,
        [FromServices] GetForumsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsCountQuery
        {
            CreatedBy = request.CreatedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}