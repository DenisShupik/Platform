using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryQuery
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public required CategoryId CategoryId { get; init; }
}

public sealed class GetCategoryQueryHandler
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoryQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    private Task<OneOf<T, CategoryNotFoundError>> HandleAsync<T>(
        GetCategoryQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetOneAsync<T>(request.CategoryId, cancellationToken);
    }

    public Task<OneOf<CategoryDto, CategoryNotFoundError>> HandleAsync(
        GetCategoryQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<CategoryDto>(request, cancellationToken);
    }
}