using CoreService.Domain.Entities;

namespace CoreService.Application.Interfaces;

public interface IPostRepository
{
    public Task AddAsync(Post post, CancellationToken cancellationToken);
}