using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetThreadsPostsCountQuery : IQuery<Dictionary<ThreadId, ulong>>
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    public required IdSet<ThreadId, Guid> ThreadIds { get; init; }
}

public sealed class
    GetThreadsPostsCountQueryHandler : IQueryHandler<GetThreadsPostsCountQuery, Dictionary<ThreadId, ulong>>
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsPostsCountQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<ThreadId, ulong>> HandleAsync(GetThreadsPostsCountQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetThreadsPostsCountAsync(request, cancellationToken);
    }
}