using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesPostsLatestQuery
{
    /// <summary>
    /// Идентификаторы разделов
    /// </summary>
    public required IdSet<CategoryId> CategoryIds { get; init; }
}

public sealed class GetCategoriesPostsLatestQueryHandler
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesPostsLatestQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    private Task<Dictionary<CategoryId,T>> HandleAsync<T>(
        GetCategoriesPostsLatestQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetCategoriesPostsLatestAsync<T>(request, cancellationToken);
    }

    public Task<Dictionary<CategoryId,PostDto>> HandleAsync(
        GetCategoriesPostsLatestQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<PostDto>(request, cancellationToken);
    }
}