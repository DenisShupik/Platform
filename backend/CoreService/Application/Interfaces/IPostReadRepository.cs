using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface IPostReadRepository
{
    public Task<OneOf<T, ThreadNotFoundError, PostNotFoundError>> GetOneAsync<T>(ThreadId threadId, PostId postId,
        CancellationToken cancellationToken);

    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetPostsQuery request, CancellationToken cancellationToken);
}