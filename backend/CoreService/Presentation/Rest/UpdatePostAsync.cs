using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    NoContent,
    NotFound<PostNotFoundError>,
    NotFound<ThreadNotFoundError>,
    Conflict<ThreadLockedByStateError>,
    Forbid<NonThreadOwnerError>,
    Forbid<InsufficientRoleToEditHeaderPostError>,
    Conflict<PostStaleError>
>;

public static partial class Api
{
    /// <summary>
    /// Обновить сообщение
    /// </summary>
    public static async Task<Response> UpdatePostAsync(
        UpdatePostRequest request,
        [FromServices] UpdatePostCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdatePostCommand
        {
            PostId = request.PostId,
            Content = request.Body.Content,
            RowVersion = request.Body.RowVersion,
            UpdatedBy = request.RequestedBy.UserId,
            UpdatedAt = DateTime.UtcNow,
            UpdaterRole = request.RequestedBy.Role,
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result
            .Match<Response>(
                _ => TypedResults.NoContent(),
                error => TypedResults.NotFound(error),
                error => TypedResults.NotFound(error),
                error => TypedResults.Conflict(error),
                error => new Forbid<NonThreadOwnerError>(error),
                error => new Forbid<InsufficientRoleToEditHeaderPostError>(error),
                error => TypedResults.Conflict(error)
            );
    }
}