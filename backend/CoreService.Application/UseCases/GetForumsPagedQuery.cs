using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public enum GetForumsPagedQuerySortType : byte
{
    ForumId = 0
}

public sealed class GetForumsPagedQuery<T> : SingleSortPagedQuery<IReadOnlyList<T>, GetForumsPagedQuerySortType>
{
    /// <summary>
    /// Название форума
    /// </summary>
    public required ForumTitle? Title { get; init; }

    /// <summary>
    /// Идентификатор пользователя, создавшего форум
    /// </summary>
    public required UserId? CreatedBy { get; init; }
}

public sealed class GetForumsPagedQueryHandler<T> : IQueryHandler<GetForumsPagedQuery<T>, IReadOnlyList<T>>
{
    private readonly IForumReadRepository _repository;

    public GetForumsPagedQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<T>> HandleAsync(GetForumsPagedQuery<T> query, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(query, cancellationToken);
    }
}