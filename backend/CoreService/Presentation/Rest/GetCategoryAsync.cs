using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<CategoryDto>, NotFound<CategoryNotFoundError>>> GetCategoryAsync(
        [FromRoute] CategoryId categoryId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoryQuery
        {
            CategoryId = categoryId
        };

        var result = await messageBus.InvokeAsync<OneOf<CategoryDto, CategoryNotFoundError>>(query, cancellationToken);

        return result.Match<Results<Ok<CategoryDto>, NotFound<CategoryNotFoundError>>>(
            categoryDto => TypedResults.Ok(categoryDto),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}