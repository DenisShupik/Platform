using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Mapster;
using OneOf;
using SharedKernel.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

public sealed class GetThreadQuery
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public required ThreadId ThreadId { get; init; }

    /// <summary>
    /// Идентификатор пользователя, запросившего данные
    /// </summary>
    public required UserId? QueriedBy { get; init; }
}

[GenerateOneOf]
public partial class GetThreadQueryResult<T> : OneOfBase<T, ThreadNotFoundError, NonThreadOwnerError>;

public sealed class GetThreadQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetThreadQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    private async Task<GetThreadQueryResult<T>> HandleAsync<T>(
        GetThreadQuery request, CancellationToken cancellationToken
    )
    {
        var threadOrError = await _repository.GetOneAsync<Thread>(request.ThreadId, cancellationToken);
        if (threadOrError.TryPickT1(out var error, out var thread)) return error;
        if (thread.Status == ThreadStatus.Draft && (request.QueriedBy == null || request.QueriedBy != thread.CreatedBy))
            return new NonThreadOwnerError(request.ThreadId);
        return thread.Adapt<T>();
    }

    public Task<GetThreadQueryResult<ThreadDto>> HandleAsync(
        GetThreadQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<ThreadDto>(request, cancellationToken);
    }
}