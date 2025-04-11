using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IPostReadRepository
{
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetPostsQuery request, CancellationToken cancellationToken);
}