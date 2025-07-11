using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface IPostRepository
{
    public Task<OneOf<PostNotFoundError, Post>> GetOneAsync(ThreadId threadId, PostId postId,
        CancellationToken cancellationToken);
}