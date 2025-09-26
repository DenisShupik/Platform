using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using OneOf;

namespace CoreService.Application.UseCases;

public enum GetCategoryThreadsPagedQuerySortType : byte
{
    Activity = 0
}

public sealed class
    GetCategoryThreadsPagedQuery<T> : SingleSortPagedQuery<OneOf<IReadOnlyList<T>,CategoryNotFoundError>, GetCategoryThreadsPagedQuerySortType>
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public required CategoryId CategoryId { get; init; }

    /// <summary>
    /// Включать ли в отбор черновики тем
    /// </summary>
    public required bool IncludeDraft { get; init; }
}

public sealed class
    GetCategoryThreadsPagedQueryHandler<T> : IQueryHandler<GetCategoryThreadsPagedQuery<T>, OneOf<IReadOnlyList<T>,CategoryNotFoundError>>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoryThreadsPagedQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public Task<OneOf<IReadOnlyList<T>,CategoryNotFoundError>> HandleAsync(GetCategoryThreadsPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        return _repository.GetCategoryThreadsAsync(query, cancellationToken);
    }
}