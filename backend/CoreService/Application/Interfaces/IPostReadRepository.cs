using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IPostReadRepository
{
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetPostsQuery request, CancellationToken cancellationToken);
    public Task<Dictionary<CategoryId,T>> GetCategoriesPostsLatestAsync<T>(GetCategoriesPostsLatestQuery request, CancellationToken cancellationToken);
    public Task<Dictionary<CategoryId,long>> GetCategoriesPostsCountAsync(GetCategoriesPostsCountQuery request, CancellationToken cancellationToken);
}