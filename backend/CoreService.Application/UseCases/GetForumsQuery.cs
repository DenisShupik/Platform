using CoreService.Application.Dtos;
using CoreService.Application.Enums;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed partial class GetForumsQuery : PagedQuery
{
    public enum GetForumsQuerySortType
    {
        LatestPost
    }

    /// <summary>
    /// Сортировка
    /// </summary>
    public required SortCriteria<GetForumsQuerySortType>? Sort { get; init; }

    /// <summary>
    /// Название форума
    /// </summary>
    public required ForumTitle? Title { get; init; }

    /// <summary>
    /// Идентификатор пользователя, создавшего форум
    /// </summary>
    public required UserId? CreatedBy { get; init; }

    public required ForumContainsFilter? Contains { get; init; }
}

public sealed class GetForumsQueryValidator : PagedQueryValidator<GetForumsQuery>
{
}

public sealed class GetForumsQueryHandler
{
    private readonly IForumReadRepository _repository;

    public GetForumsQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(GetForumsQuery query, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync<T>(query, cancellationToken);
    }

    public async Task<IReadOnlyList<ForumDto>> HandleAsync(GetForumsQuery query, CancellationToken cancellationToken)
    {
        return await HandleAsync<ForumDto>(query, cancellationToken);
    }
}