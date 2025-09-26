using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesPostsLatestQuery<T> : IQuery<Dictionary<CategoryId, T>>
{
    /// <summary>
    /// Идентификаторы разделов
    /// </summary>
    public required IdSet<CategoryId, Guid> CategoryIds { get; init; }
}

public sealed class
    GetCategoriesPostsLatestQueryHandler<T> : IQueryHandler<GetCategoriesPostsLatestQuery<T>,
    Dictionary<CategoryId, T>>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesPostsLatestQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<CategoryId, T>> HandleAsync(GetCategoriesPostsLatestQuery<T> query,
        CancellationToken cancellationToken
    )
    {
        return _repository.GetCategoriesPostsLatestAsync(query, cancellationToken);
    }
}