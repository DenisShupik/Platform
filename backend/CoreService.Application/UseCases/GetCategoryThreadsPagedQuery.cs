using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using Shared.Domain.Abstractions.Results;
using CoreService.Domain.ValueObjects;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Application.UseCases;

public enum GetCategoryThreadsPagedQuerySortType : byte
{
    Activity = 0
}

public sealed class
    GetCategoryThreadsPagedQuery<T> : SingleSortPagedQuery<Result<IReadOnlyList<T>, CategoryNotFoundError>,
    GetCategoryThreadsPagedQuerySortType>
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public required CategoryId CategoryId { get; init; }

    /// <summary>
    /// Состояние темы
    /// </summary>
    public required ThreadState? State { get; init; }

    public required ActorContext? QueriedBy { get; init; }
}

public sealed class
    GetCategoryThreadsPagedQueryHandler<T> : IQueryHandler<GetCategoryThreadsPagedQuery<T>,
    Result<IReadOnlyList<T>, CategoryNotFoundError>>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoryThreadsPagedQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Result<IReadOnlyList<T>, CategoryNotFoundError>> HandleAsync(GetCategoryThreadsPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        return _repository.GetCategoryThreadsAsync(query, cancellationToken);
    }
}
