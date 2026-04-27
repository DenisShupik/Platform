using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Created<PostId>,
    NotFound<ThreadNotFoundError>,
    Conflict<ThreadLockedByStateError>,
    Forbid<NonThreadOwnerError>,
    Conflict<PostLimitReachedError>
>;

public static partial class Api
{
    /// <summary>
    /// Создать сообщение
    /// </summary>
    public static async Task<Response> CreatePostAsync(
        CreatePostRequest request,
        [FromServices] CreatePostCommandHandler handler,
        [FromServices] LinkGenerator linker,
        CancellationToken cancellationToken
    )
    {
        var command = new CreatePostCommand
        {
            ThreadId = request.ThreadId,
            Content = request.Body.Content,
            CreatedBy = request.RequestedBy.UserId,
            CreatorRole = request.RequestedBy.Role,
            CreatedAt = DateTime.UtcNow
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result
            .Match<Response>(
                postId => {
                    var path = linker.GetPathByName(
                        nameof(GetPostAsync), 
                        new { postId }
                    );
                    
                    return TypedResults.Created(path, postId);
                },
                error => TypedResults.NotFound(error),
                error => TypedResults.Conflict(error),
                error => new Forbid<NonThreadOwnerError>(error),
                error => TypedResults.Conflict(error)
            );
    }
}