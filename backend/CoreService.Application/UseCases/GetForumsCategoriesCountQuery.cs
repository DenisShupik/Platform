using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetForumsCategoriesCountQuery : IQuery<Dictionary<ForumId, ulong>>
{
    /// <summary>
    /// Идентификаторы форумов
    /// </summary>
    public required IdSet<ForumId, Guid> ForumIds { get; init; }
}

public sealed class
    GetForumsCategoriesCountQueryHandler : IQueryHandler<GetForumsCategoriesCountQuery, Dictionary<ForumId, ulong>>
{
    private readonly IForumReadRepository _repository;

    public GetForumsCategoriesCountQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<ForumId, ulong>> HandleAsync(GetForumsCategoriesCountQuery query,
        CancellationToken cancellationToken
    )
    {
        return _repository.GetForumsCategoriesCountAsync(query, cancellationToken);
    }
}