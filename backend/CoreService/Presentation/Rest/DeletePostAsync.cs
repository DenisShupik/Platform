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
    Forbid<NonPostAuthorError>,
    Forbid<ApprovedHeaderPostDeletionForbiddenError>
>;

public static partial class Api
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/operation[@key='deletePost']/*" />
    public static async Task<Response> DeletePostAsync(
        DeletePostRequest request,
        [FromServices] DeletePostCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new DeletePostCommand
        {
            PostId = request.PostId,
            DeletedBy = request.RequestedBy.UserId,
            DeleterRole = request.RequestedBy.Role,
            DeletedAt = DateTime.UtcNow
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result
            .Match<Response>(
                _ => TypedResults.NoContent(),
                error => TypedResults.NotFound(error),
                error => TypedResults.NotFound(error),
                error => TypedResults.Conflict(error),
                error => new Forbid<NonPostAuthorError>(error),
                error => new Forbid<ApprovedHeaderPostDeletionForbiddenError>(error)
            );
    }
}
