using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetForumsCategoriesLatestQuery
{
    /// <summary>
    /// Идентификаторы форумов
    /// </summary>
    public required IdSet<ForumId> ForumIds { get; init; }
    public required int Count { get; init; }
}

public sealed class GetForumsCategoriesLatestQueryHandler
{
    private readonly IForumReadRepository _repository;

    public GetForumsCategoriesLatestQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    private Task<Dictionary<ForumId, T[]>> HandleAsync<T>(
        GetForumsCategoriesLatestQuery request, CancellationToken cancellationToken
    ) where T : IHasForumId
    {
        return _repository.GetForumsCategoriesLatestAsync<T>(request, cancellationToken);
    }

    public Task<Dictionary<ForumId, CategoryDto[]>> HandleAsync(
        GetForumsCategoriesLatestQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<CategoryDto>(request, cancellationToken);
    }
}