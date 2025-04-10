using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using SharedKernel.Application.Abstractions;
using SharedKernel.Sorting;

namespace CoreService.Application.UseCases;

public sealed class GetForumsQuery : PaginatedQuery
{
    public enum SortType
    {
        LatestPost
    }
    
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public required SortCriteria<SortType>? Sort { get; init; }
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