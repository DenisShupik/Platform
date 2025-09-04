using CoreService.Application.Dtos;
using CoreService.Application.Enums;
using CoreService.Application.Interfaces;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetActivitiesPagedQuery : MultiSortPagedQuery<GetActivitiesPagedQuery.SortType>
{
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

    public enum SortType : byte
    {
        Latest = 0
    }

    public required ActivityType Activity { get; init; }
    public required GetActivitiesPagedQueryGroupByType GetActivitiesPagedQueryGroupBy { get; init; }
    public required GetActivitiesPagedQueryModeType GetActivitiesPagedQueryMode { get; init; }
}

public sealed class GetActivitiesPagedQueryHandler
{
    private readonly IActivityReadRepository _repository;

    public GetActivitiesPagedQueryHandler(IActivityReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(GetActivitiesPagedQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync<T>(request, cancellationToken);
    }

    public async Task<IReadOnlyList<ActivityDto>> HandleAsync(GetActivitiesPagedQuery request,
        CancellationToken cancellationToken)
    {
        return await HandleAsync<ActivityDto>(request, cancellationToken);
    }
}