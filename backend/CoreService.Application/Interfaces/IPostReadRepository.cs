using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface IPostReadRepository
{
    public Task<OneOf<T, PostNotFoundError>> GetOneAsync<T>(PostId postId, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetThreadPostsAsync<T>(GetThreadPostsPagedQuery request, CancellationToken cancellationToken);
}