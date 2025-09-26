using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using OneOf;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

public enum GetThreadPostsPagedQuerySortType : byte
{
    Index = 0
}

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
public sealed partial class
    GetThreadPostsPagedQuery<T> : SingleSortPagedQuery<OneOf<IReadOnlyList<T>, ThreadNotFoundError>, GetThreadPostsPagedQuerySortType>;

public sealed class
    GetThreadPostsPagedQueryHandler<T> : IQueryHandler<GetThreadPostsPagedQuery<T>, OneOf<IReadOnlyList<T>, ThreadNotFoundError>>
{
    private readonly IPostReadRepository _repository;

    public GetThreadPostsPagedQueryHandler(IPostReadRepository repository)
    {
        _repository = repository;
    }

    public Task<OneOf<IReadOnlyList<T>, ThreadNotFoundError>> HandleAsync(GetThreadPostsPagedQuery<T> request, CancellationToken cancellationToken)
    {
        return _repository.GetThreadPostsAsync(request, cancellationToken);
    }
}