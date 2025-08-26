using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface ICategoryReadRepository
{
    public Task<OneOf<T, CategoryNotFoundError>> GetOneAsync<T>(CategoryId id, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetBulkAsync<T>(List<CategoryId> ids, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesPagedQuery request, CancellationToken cancellationToken);

    public Task<Dictionary<CategoryId, long>> GetCategoriesThreadsCountAsync(GetCategoriesThreadsCountQuery request,
        CancellationToken cancellationToken);

    public Task<IReadOnlyList<T>> GetCategoryThreadsAsync<T>(GetCategoryThreadsQuery request,
        CancellationToken cancellationToken);

    public Task<Dictionary<CategoryId, long>> GetCategoriesPostsCountAsync(GetCategoriesPostsCountQuery request,
        CancellationToken cancellationToken);

    public Task<Dictionary<CategoryId, T>> GetCategoriesPostsLatestAsync<T>(GetCategoriesPostsLatestQuery request,
        CancellationToken cancellationToken);
}