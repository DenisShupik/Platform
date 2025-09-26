using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using Mapster;
using OneOf;
using Shared.Application.Interfaces;
using UserService.Domain.Enums;
using UserService.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
public sealed partial class GetThreadQuery<T> : IQuery<OneOf<T, ThreadNotFoundError, NonThreadOwnerError>>
{
    /// <summary>
    /// Идентификатор пользователя, запросившего данные
    /// </summary>
    public required UserId? QueriedBy { get; init; }

    public required RoleType Role { get; init; }
}

public sealed class
    GetThreadQueryHandler<T> : IQueryHandler<GetThreadQuery<T>, OneOf<T, ThreadNotFoundError, NonThreadOwnerError>>
{
    private readonly IThreadReadRepository _repository;

    public GetThreadQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<T, ThreadNotFoundError, NonThreadOwnerError>> HandleAsync(
        GetThreadQuery<T> query, CancellationToken cancellationToken
    )
    {
        var threadOrError = await _repository.GetOneAsync<Thread>(query.ThreadId, cancellationToken);
        if (threadOrError.TryPickT1(out var error, out var thread)) return error;
        if (thread.Status == ThreadStatus.Draft && query.Role == RoleType.User &&
            (query.QueriedBy == null || query.QueriedBy != thread.CreatedBy))
            return new NonThreadOwnerError(query.ThreadId);
        return thread.Adapt<T>();
    }
}