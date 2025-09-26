using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetForumsBulkQuery<T> : IQuery<IReadOnlyList<T>>
{
    /// <summary>
    /// Идентификаторы форумов
    /// </summary>
    public required IdSet<ForumId, Guid> ForumIds { get; init; }
}

public sealed class GetForumsBulkQueryHandler<T> : IQueryHandler<GetForumsBulkQuery<T>, IReadOnlyList<T>>
{
    private readonly IForumReadRepository _repository;

    public GetForumsBulkQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<T>> HandleAsync(GetForumsBulkQuery<T> query, CancellationToken cancellationToken) =>
        _repository.GetBulkAsync<T>(query.ForumIds, cancellationToken);
}