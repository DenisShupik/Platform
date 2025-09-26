using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetThreadsBulkQuery<T> : IQuery<IReadOnlyList<T>>
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    public required IdSet<ThreadId, Guid> ThreadIds { get; init; }
}

public sealed class GetThreadsBulkQueryHandler<T> : IQueryHandler<GetThreadsBulkQuery<T>, IReadOnlyList<T>>
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsBulkQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<T>> HandleAsync(GetThreadsBulkQuery<T> query, CancellationToken cancellationToken) =>
        _repository.GetBulkAsync<T>(query.ThreadIds, cancellationToken);
}