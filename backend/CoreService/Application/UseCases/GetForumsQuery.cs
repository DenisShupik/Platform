using CoreService.Application.Dtos;
using CoreService.Application.Enums;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed partial class GetForumsQuery : PaginatedQuery
{
    public enum SortType
    {
        LatestPost
    }

    /// <summary>
    /// Сортировка
    /// </summary>
    public required SortCriteria<SortType>? Sort { get; init; }

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

public sealed class GetForumsQueryValidator : PaginatedQueryValidator<GetForumsQuery>
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