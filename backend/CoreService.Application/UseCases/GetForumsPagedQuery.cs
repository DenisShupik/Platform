using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetForumsPagedQuery : PagedQuery
{
    public enum GetForumsPagedQuerySortType
    {
        ForumId
    }

    /// <summary>
    /// Сортировка
    /// </summary>
    public required SortCriteria<GetForumsPagedQuerySortType>? Sort { get; init; }

    /// <summary>
    /// Название форума
    /// </summary>
    public required ForumTitle? Title { get; init; }

    /// <summary>
    /// Идентификатор пользователя, создавшего форум
    /// </summary>
    public required UserId? CreatedBy { get; init; }
}

public sealed class GetForumsPagedQueryValidator : PagedQueryValidator<GetForumsPagedQuery>;

public sealed class GetForumsPagedQueryHandler
{
    private readonly IForumReadRepository _repository;

    public GetForumsPagedQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(GetForumsPagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync<T>(pagedQuery, cancellationToken);
    }

    public async Task<GetForumsPagedQueryResult> HandleAsync(GetForumsPagedQuery pagedQuery,
        CancellationToken cancellationToken)
    {
        return await HandleAsync<ForumDto>(pagedQuery, cancellationToken);
    }
}