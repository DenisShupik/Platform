using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetThreadsBulkQuery
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    public required IdSet<ThreadId> ThreadIds { get; init; }
}

public sealed class GetThreadsBulkQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsBulkQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(HashSet<ThreadId> ids, CancellationToken cancellationToken) =>
        _repository.GetBulkAsync<T>(ids, cancellationToken);

    public Task<IReadOnlyList<ThreadDto>> HandleAsync(GetThreadsBulkQuery query, CancellationToken cancellationToken) =>
        HandleAsync<ThreadDto>(query.ThreadIds, cancellationToken);
}