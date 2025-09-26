using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;
using Shared.Application.Interfaces;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryQuery<T> : IQuery<OneOf<T, CategoryNotFoundError>>
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public required CategoryId CategoryId { get; init; }
}

public sealed class GetCategoryQueryHandler<T> : IQueryHandler<GetCategoryQuery<T>, OneOf<T, CategoryNotFoundError>>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoryQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public Task<OneOf<T, CategoryNotFoundError>> HandleAsync(
        GetCategoryQuery<T> query, CancellationToken cancellationToken
    )
    {
        return _repository.GetOneAsync<T>(query.CategoryId, cancellationToken);
    }
}