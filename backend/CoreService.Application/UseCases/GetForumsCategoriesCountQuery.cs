using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using Shared.Domain.Abstractions.Results;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using QueryResult = Dictionary<ForumId, Result<Count, ForumNotFoundError>>;

public sealed class GetForumsCategoriesCountQuery : IQuery<QueryResult>
{
    /// <summary>
    /// Идентификаторы форумов
    /// </summary>
    public required IdSet<ForumId, Guid> ForumIds { get; init; }

    public required ActorContext? QueriedBy { get; init; }
}

public sealed class
    GetForumsCategoriesCountQueryHandler : IQueryHandler<GetForumsCategoriesCountQuery, QueryResult>
{
    private readonly IForumReadRepository _repository;

    public GetForumsCategoriesCountQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public Task<QueryResult> HandleAsync(GetForumsCategoriesCountQuery query,
        CancellationToken cancellationToken
    )
    {
        return _repository.GetForumsCategoriesCountAsync(query, cancellationToken);
    }
}
