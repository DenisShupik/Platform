using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class
    GetCategoriesBulkQuery<T> : IQuery<Dictionary<CategoryId, Result<T, CategoryNotFoundError>>>
    where T : notnull
{
    /// <summary>
    /// Идентификаторы разделов
    /// </summary>
    public required IdSet<CategoryId, Guid> CategoryIds { get; init; }

    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class GetCategoriesBulkQueryHandler<T> : IQueryHandler<GetCategoriesBulkQuery<T>,
    Dictionary<CategoryId, Result<T, CategoryNotFoundError>>>
    where T : notnull
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesBulkQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<CategoryId, Result<T, CategoryNotFoundError>>> HandleAsync(
        GetCategoriesBulkQuery<T> query, CancellationToken cancellationToken) =>
        _repository.GetBulkAsync(query, cancellationToken);
}