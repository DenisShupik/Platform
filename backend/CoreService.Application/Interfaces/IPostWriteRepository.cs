using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface IPostWriteRepository
{
    public Task<Result<Post, PostNotFoundError>> GetOneAsync(PostId postId, CancellationToken cancellationToken);
}