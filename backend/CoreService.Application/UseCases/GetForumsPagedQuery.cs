using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Interfaces;
using SharedKernel.Application.ValueObjects;
using UserService.Domain.ValueObjects;
using Vogen;

namespace CoreService.Application.UseCases;

[ValueObject<int>]
public readonly partial struct GetForumsPagedQueryLimit : IPaginationLimit, IVogen<GetForumsPagedQueryLimit, int>
{
    public static int Min => 10;
    public static int Max => 100;
    public static int Default => 10;

    private static Validation Validate(in int value) =>
        SharedKernel.Application.Helpers.ValidationHelper.LimitValidation<GetForumsPagedQueryLimit>(value);
}

public sealed class GetForumsPagedQuery : IPagination<GetForumsPagedQueryLimit>
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

    public required PaginationOffset? Offset { get; init; }
    public required GetForumsPagedQueryLimit? Limit { get; init; }
}

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