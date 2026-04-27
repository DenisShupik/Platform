using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Errors;
using Microsoft.AspNetCore.Routing;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Created<CategoryId>,
    Unauthorized<AuthenticationRequiredError>,
    Unauthorized<ClaimNotFoundError>,
    Forbid<PermissionDeniedError>,
    NotFound<ForumNotFoundError>
>;

public static partial class Api
{
    /// <summary>
    /// Создать раздел
    /// </summary>
    public static async Task<Response> CreateCategoryAsync(
        CreateCategoryRequest request,
        [FromServices] CreateCategoryCommandHandler handler,
        [FromServices] LinkGenerator linker,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateCategoryCommand
        {
            ForumId = request.Body.ForumId,
            Title = request.Body.Title,
            CreatedBy = request.RequestedBy.UserId,
            CreatorRole = request.RequestedBy.Role,
            CreatedAt = DateTime.UtcNow
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            categoryId => {
                var path = linker.GetPathByName(
                    nameof(GetCategoryAsync), 
                    new { categoryId }
                );
                
                return TypedResults.Created(path, categoryId);
            },
            error => new Forbid<PermissionDeniedError>(error),
            error => TypedResults.NotFound(error)
        );
    }
}