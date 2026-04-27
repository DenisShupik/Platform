using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
public sealed partial class
    GetThreadQuery<T> : IQuery<Result<T, ThreadNotFoundError, PermissionDeniedError>>
    where T : notnull
{
    /// <summary>
    /// Идентификатор пользователя, запросившего данные
    /// </summary>
    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class
    GetThreadQueryHandler<T> : IQueryHandler<GetThreadQuery<T>,
    Result<T, ThreadNotFoundError, PermissionDeniedError>>
    where T : notnull
{
    private readonly IThreadReadRepository _repository;

    public GetThreadQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Result<T, ThreadNotFoundError, PermissionDeniedError>> HandleAsync(
        GetThreadQuery<T> query, CancellationToken cancellationToken
    )
    {
        return _repository.GetOneAsync(query, cancellationToken);
    }
}