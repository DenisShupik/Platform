using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Errors;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Created<ForumId>,
    Unauthorized<AuthenticationRequiredError>,
    Unauthorized<ClaimNotFoundError>,
    Forbid<PermissionDeniedError>
>;

public static partial class Api
{
    /// <summary>
    /// Создать форум
    /// </summary>
    public static async Task<Response> CreateForumAsync(
        CreateForumRequest request,
        [FromServices] CreateForumCommandHandler handler,
        [FromServices] LinkGenerator linker,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateForumCommand
        {
            Title = request.Body.Title,
            CreatedBy = request.RequestedBy.UserId,
            CreatorRole = request.RequestedBy.Role,
            CreatedAt = DateTime.UtcNow
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            forumId => {
                var path = linker.GetPathByName(
                    nameof(GetForumAsync), 
                    new { forumId }
                );
                
                return TypedResults.Created(path, forumId);
            },
            error => new Forbid<PermissionDeniedError>(error)
        );
    }
}