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
    Forbid<PermissionDeniedError>,
    Conflict<DuplicatePostBookmarkError>
>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='createPostBookmark']/*" />
    public static async Task<Response> CreatePostBookmarkAsync(
        CreatePostBookmarkRequest request,
        [FromServices] CreatePostBookmarkCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new CreatePostBookmarkCommand
        {
            UserId = request.RequestedBy.UserId,
            PostId = request.PostId,
            CreatedBy = request.RequestedBy,
            CreatedAt = DateTime.UtcNow
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            _ => TypedResults.NoContent(),
            error => TypedResults.NotFound(error),
            error => new Forbid<PermissionDeniedError>(error),
            error => TypedResults.Conflict(error)
        );
    }
}
