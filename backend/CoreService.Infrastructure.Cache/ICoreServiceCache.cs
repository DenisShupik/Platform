using CoreService.Application.Dtos;
using CoreService.Domain.ValueObjects;

namespace CoreService.Infrastructure.Cache;

public interface ICoreServiceCache
{
    ValueTask<ThreadDto> GetOrSetAsync(ThreadId threadId, Func<CancellationToken, Task<ThreadDto>> factory,
        CancellationToken cancellationToken);

    ValueTask<PostDto> GetOrSetAsync(ThreadId threadId, PostId postId, Func<CancellationToken, Task<PostDto>> factory,
        CancellationToken cancellationToken);
}