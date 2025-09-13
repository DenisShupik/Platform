using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Application.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryThreadsPagedQuery : SingleSortPagedQuery<GetCategoryThreadsPagedQuery.SortType>
{
    public enum SortType : byte
    {
        Activity = 0
    }

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public required CategoryId CategoryId { get; init; }

    /// <summary>
    /// Включать ли в отбор черновики тем
    /// </summary>
    public required bool IncludeDraft { get; init; }
}

public sealed class GetCategoryThreadsQueryHandler
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoryThreadsQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(
        GetCategoryThreadsPagedQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetCategoryThreadsAsync<T>(request, cancellationToken);
    }

    public Task<IReadOnlyList<ThreadDto>> HandleAsync(
        GetCategoryThreadsPagedQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<ThreadDto>(request, cancellationToken);
    }
}