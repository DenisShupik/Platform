using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public enum GetThreadsPagedQuerySortType : byte
{
    ThreadId = 0
}

public sealed class
    GetThreadsPagedQuery<T> : SingleSortPagedQuery<Result<List<T>, NotAdminError, NotOwnerError>,
    GetThreadsPagedQuerySortType>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required UserId? CreatedBy { get; init; }

    /// <summary>
    /// Состояние темы
    /// </summary>
    public required ThreadStatus? Status { get; init; }

    /// <summary>
    /// Идентификатор пользователя, запросившего данные
    /// </summary>
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetThreadsPagedQueryHandler<T> : IQueryHandler<GetThreadsPagedQuery<T>,
    Result<List<T>, NotAdminError, NotOwnerError>>
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsPagedQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<T>, NotAdminError, NotOwnerError>> HandleAsync(
        GetThreadsPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        if (query.Status == ThreadStatus.Draft)
        {
            if (query.QueriedBy == null || query.CreatedBy == null) return new NotAdminError();

            if (query.CreatedBy != query.QueriedBy)
            {
                return new NotOwnerError();
            }
        }

        return await _repository.GetAllAsync(query, cancellationToken);
    }
}