using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;

namespace CoreService.Application.Interfaces;

public interface IPostReadRepository
{
    public Task<Result<T, PostNotFoundError>> GetOneAsync<T>(PostId postId, CancellationToken cancellationToken);
    public Task<Result<IReadOnlyList<T>, ThreadNotFoundError>> GetThreadPostsAsync<T>(GetThreadPostsPagedQuery<T> request, CancellationToken cancellationToken);
}