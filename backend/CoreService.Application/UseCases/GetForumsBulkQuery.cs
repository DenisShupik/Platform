using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetForumsBulkQuery
{
    /// <summary>
    /// Идентификаторы форумов
    /// </summary>
    public required IdSet<ForumId> ForumIds { get; init; }
}

public sealed class GetForumsBulkQueryHandler
{
    private readonly IForumReadRepository _repository;

    public GetForumsBulkQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(IdSet<ForumId> ids, CancellationToken cancellationToken) =>
        _repository.GetBulkAsync<T>(ids, cancellationToken);

    public Task<GetForumsBulkQueryResult> HandleAsync(GetForumsBulkQuery query, CancellationToken cancellationToken) =>
        HandleAsync<ForumDto>(query.ForumIds, cancellationToken);
}