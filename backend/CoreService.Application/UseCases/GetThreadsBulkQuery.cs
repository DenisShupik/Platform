using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class
    GetThreadsBulkQuery<T> : IQuery<Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError>>>
    where T : notnull
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    public required IdSet<ThreadId, Guid> ThreadIds { get; init; }

    /// <summary>
    /// Идентификатор пользователя, запросившего данные
    /// </summary>
    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class GetThreadsBulkQueryHandler<T> : IQueryHandler<GetThreadsBulkQuery<T>,
    Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError>>> where T : notnull
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsBulkQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError>>> HandleAsync(GetThreadsBulkQuery<T> query, CancellationToken cancellationToken) =>
        _repository.GetBulkAsync(query, cancellationToken);
}