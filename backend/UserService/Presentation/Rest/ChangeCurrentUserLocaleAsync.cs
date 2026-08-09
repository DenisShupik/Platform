using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Errors;
using UserService.Application.UseCases;
using UserService.Presentation.Rest.Dtos;

namespace UserService.Presentation.Rest;

using Response = Results<NoContent, NotFound<UserNotFoundError>>;

public static partial class Api
{
    public static async Task<Response> ChangeCurrentUserLocaleAsync(
        ChangeCurrentUserLocaleRequest request,
        [FromServices] ChangeCurrentUserLocaleCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new ChangeCurrentUserLocaleCommand
        {
            UserId = request.RequestedBy.UserId,
            Locale = request.Body.Locale
        };

        var result = await handler.HandleAsync(command, cancellationToken);
        return result.Match<Response>(
            _ => TypedResults.NoContent(),
            error => TypedResults.NotFound(error));
    }
}
