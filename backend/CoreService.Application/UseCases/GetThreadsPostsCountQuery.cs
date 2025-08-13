using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetThreadsPostsCountQuery
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    public required IdSet<ThreadId> ThreadIds { get; init; }
}

public sealed class GetThreadsPostsCountQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsPostsCountQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<ThreadId, long>> HandleAsync(GetThreadsPostsCountQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetThreadsPostsCountAsync(request, cancellationToken);
    }
}