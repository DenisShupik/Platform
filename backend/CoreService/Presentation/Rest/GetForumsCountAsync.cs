using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.ValueObjects;

namespace CoreService.Presentation.Rest;

using Response = Ok<Count>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='getForumsCount']/*" />
    public static async Task<Response> GetForumsCountAsync(
        GetForumsCountRequest request,
        [FromServices] GetForumsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsCountQuery
        {
            CreatedBy = request.CreatedBy,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
