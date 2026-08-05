using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

using Response = Results<NoContent, NotFound<PostBookmarkNotFoundError>>;

public static partial class Api
{
    /// <summary>
    /// Удалить сообщение из закладок
    /// </summary>
    public static async Task<Response> DeletePostBookmarkAsync(
        DeletePostBookmarkRequest request,
        [FromServices] DeletePostBookmarkCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new DeletePostBookmarkCommand
        {
            UserId = request.RequestedBy.UserId,
            PostId = request.PostId
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            _ => TypedResults.NoContent(),
            error => TypedResults.NotFound(error)
        );
    }
}
