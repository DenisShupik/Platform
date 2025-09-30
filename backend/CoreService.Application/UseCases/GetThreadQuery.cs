using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using Mapster;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.Enums;
using UserService.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
public sealed partial class GetThreadQuery<T> : IQuery<Result<T, ThreadNotFoundError, NonThreadOwnerError>>
    where T : notnull
{
    /// <summary>
    /// Идентификатор пользователя, запросившего данные
    /// </summary>
    public required UserId? QueriedBy { get; init; }

    public required RoleType Role { get; init; }
}

public sealed class
    GetThreadQueryHandler<T> : IQueryHandler<GetThreadQuery<T>, Result<T, ThreadNotFoundError, NonThreadOwnerError>>
    where T : notnull
{
    private readonly IThreadReadRepository _repository;

    public GetThreadQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<T, ThreadNotFoundError, NonThreadOwnerError>> HandleAsync(
        GetThreadQuery<T> query, CancellationToken cancellationToken
    )
    {
        var threadOrError = await _repository.GetOneAsync<Thread>(query.ThreadId, cancellationToken);
        if (!threadOrError.TryGet(out var thread, out var error)) return error;
        if (thread.Status == ThreadStatus.Draft && query.Role == RoleType.User &&
            (query.QueriedBy == null || query.QueriedBy != thread.CreatedBy))
            return new NonThreadOwnerError(query.ThreadId);
        return thread.Adapt<T>();
    }
}