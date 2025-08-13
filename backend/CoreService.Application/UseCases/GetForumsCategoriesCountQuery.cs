using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetForumsCategoriesCountQuery
{
    /// <summary>
    /// Идентификаторы форумов
    /// </summary>
    public required IdSet<ForumId> ForumIds { get; init; }
}

public sealed class GetForumsCategoriesCountQueryHandler
{
    private readonly IForumReadRepository _repository;

    public GetForumsCategoriesCountQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<ForumId, long>> HandleAsync(GetForumsCategoriesCountQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetForumsCategoriesCountAsync(request, cancellationToken);
    }
}