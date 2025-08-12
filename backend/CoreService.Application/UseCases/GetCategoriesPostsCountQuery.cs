using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesPostsCountQuery
{
    /// <summary>
    /// Идентификаторы разделов
    /// </summary>
    public required IdSet<CategoryId> CategoryIds { get; init; }
}

public sealed class GetCategoriesPostsCountQueryHandler
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesPostsCountQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<CategoryId, long>> HandleAsync(GetCategoriesPostsCountQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetCategoriesPostsCountAsync(request, cancellationToken);
    }
}