using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface IPostWriteRepository
{
    Task<Result<Post, PostNotFoundError>> GetOneAsync(PostId postId, CancellationToken cancellationToken);
    public void Add(Post post, string searchText);
    public void SetSearchText(Post post, string searchText);
    public void Remove(Post post);
}
