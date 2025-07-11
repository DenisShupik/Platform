using CoreService.Application.Dtos;
using CoreService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface ICoreServiceClient
{
    ValueTask<ThreadDto> GetThreadAsync(ThreadId threadId, CancellationToken cancellationToken);
    ValueTask<PostDto> GetPostAsync(ThreadId threadId,PostId postId, CancellationToken cancellationToken);
}