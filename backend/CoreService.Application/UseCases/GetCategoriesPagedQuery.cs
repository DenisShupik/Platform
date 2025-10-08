using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public enum GetCategoriesPagedQuerySortType : byte
{
    CategoryId = 0,
    ForumId = 1
}

public sealed class GetCategoriesPagedQuery<T> : MultiSortPagedQuery<IReadOnlyList<T>, GetCategoriesPagedQuerySortType>
{
    /// <summary>
    /// Идентификаторы форумов
    /// </summary>
    public required IdSet<ForumId, Guid>? ForumIds { get; init; }

    /// <summary>
    /// Название раздела
    /// </summary>
    public required CategoryTitle? Title { get; init; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetCategoriesPagedQueryHandler<T> : IQueryHandler<GetCategoriesPagedQuery<T>, IReadOnlyList<T>>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesPagedQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<T>> HandleAsync(GetCategoriesPagedQuery<T> query, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(query, cancellationToken);
    }
}