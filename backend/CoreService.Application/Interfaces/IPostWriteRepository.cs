using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface IPostWriteRepository
{
    public Task<OneOf<Post, PostNotFoundError>> GetOneAsync(PostId postId, CancellationToken cancellationToken);
}