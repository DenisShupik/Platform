using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using Generator.Attributes;
using SharedKernel.Application.Abstractions;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
public sealed partial class GetThreadPostsPagedQuery : PagedQuery
{
    public enum GetThreadPostsPagedQuerySortType
    {
        Index
    }

    /// <summary>
    /// Сортировка
    /// </summary>
    public required SortCriteria<GetThreadPostsPagedQuerySortType> Sort { get; init; }
}

public sealed class GetThreadPostsPagedQueryValidator : PagedQueryValidator<GetThreadPostsPagedQuery>;

public sealed class GetThreadPostsPagedQueryHandler
{
    private readonly IPostReadRepository _repository;

    public GetThreadPostsPagedQueryHandler(IPostReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(GetThreadPostsPagedQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetThreadPostsAsync<T>(request, cancellationToken);
    }

    public async Task<IReadOnlyList<PostDto>> HandleAsync(GetThreadPostsPagedQuery request,
        CancellationToken cancellationToken)
    {
        return await HandleAsync<PostDto>(request, cancellationToken);
    }
}