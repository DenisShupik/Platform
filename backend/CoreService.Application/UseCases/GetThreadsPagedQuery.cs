using CoreService.Application.Interfaces;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Application.UseCases;

public enum GetThreadsPagedQuerySortType : byte
{
    ThreadId = 0
}

public sealed class GetThreadsPagedQuery<T> : SingleSortPagedQuery<List<T>, GetThreadsPagedQuerySortType>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required UserId? CreatedBy { get; init; }

    /// <summary>
    /// Статус темы
    /// </summary>
    public required ThreadState? Status { get; init; }
    
    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class GetThreadsPagedQueryHandler<T> : IQueryHandler<GetThreadsPagedQuery<T>, List<T>>
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsPagedQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public Task<List<T>> HandleAsync(GetThreadsPagedQuery<T> query, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(query, cancellationToken);
    }
}