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
    Created<ThreadId>,
    Forbid<PermissionDeniedError>,
    NotFound<CategoryNotFoundError>
>;

public static partial class Api
{
    /// <summary>
    /// Создать тему
    /// </summary>
    public static async Task<Response> CreateThreadAsync(
        CreateThreadRequest request,
        [FromServices] CreateThreadCommandHandler handler,
        [FromServices] LinkGenerator linker,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateThreadCommand
        {
            CategoryId = request.Body.CategoryId,
            Title = request.Body.Title,
            CreatedBy = request.RequestedBy.UserId,
            CreatorRole = request.RequestedBy.Role,
            CreatedAt = DateTime.UtcNow
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            threadId => {
                var path = linker.GetPathByName(
                    nameof(GetThreadAsync), 
                    new { threadId }
                );
                
                return TypedResults.Created(path, threadId);
            },
            error => new Forbid<PermissionDeniedError>(error),
            error => TypedResults.NotFound(error)
        );
    }
}