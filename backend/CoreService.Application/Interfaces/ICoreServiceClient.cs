using CoreService.Application.Dtos;
using CoreService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface ICoreServiceClient
{
    ValueTask<ThreadDto> GetThreadAsync(ThreadId threadId, CancellationToken cancellationToken);
    ValueTask<IReadOnlyList<ThreadDto>> GetThreadsAsync(ISet<ThreadId> userIds, CancellationToken cancellationToken);
    ValueTask<PostDto> GetPostAsync(ThreadId threadId, PostId postId, CancellationToken cancellationToken);
}