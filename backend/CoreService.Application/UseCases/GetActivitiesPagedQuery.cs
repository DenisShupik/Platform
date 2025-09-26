using CoreService.Application.Enums;
using CoreService.Application.Interfaces;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;

namespace CoreService.Application.UseCases;

public enum GetActivitiesPagedQueryModeType
{
    Latest = 0
}

public enum GetActivitiesPagedQueryGroupByType
{
    Forum = 0,
    Category = 1,
    Thread = 2
}

public enum GetActivitiesPagedQuerySortType : byte
{
    Latest = 0
}

public sealed class GetActivitiesPagedQuery<T> : MultiSortPagedQuery<IReadOnlyList<T>, GetActivitiesPagedQuerySortType>
{
    public required ActivityType Activity { get; init; }
    public required GetActivitiesPagedQueryGroupByType GetActivitiesPagedQueryGroupBy { get; init; }
    public required GetActivitiesPagedQueryModeType GetActivitiesPagedQueryMode { get; init; }
}

public sealed class GetActivitiesPagedQueryHandler<T>: IQueryHandler<GetActivitiesPagedQuery<T>, IReadOnlyList<T>>
{
    private readonly IActivityReadRepository _repository;

    public GetActivitiesPagedQueryHandler(IActivityReadRepository repository)
    {
        _repository = repository;
    }
    
    public Task<IReadOnlyList<T>> HandleAsync(GetActivitiesPagedQuery<T> query, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(query, cancellationToken);
    }
}