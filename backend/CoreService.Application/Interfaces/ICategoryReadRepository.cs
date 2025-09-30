using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface ICategoryReadRepository
{
    public Task<Result<T, CategoryNotFoundError>> GetOneAsync<T>(CategoryId id, CancellationToken cancellationToken) where T : notnull;
    public Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<CategoryId, Guid> ids, CancellationToken cancellationToken);

    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesPagedQuery<T> request,
        CancellationToken cancellationToken);

    public Task<Dictionary<CategoryId, ulong>> GetCategoriesThreadsCountAsync(GetCategoriesThreadsCountQuery request,
        CancellationToken cancellationToken);

    public Task<Result<IReadOnlyList<T>, CategoryNotFoundError>> GetCategoryThreadsAsync<T>(
        GetCategoryThreadsPagedQuery<T> request,
        CancellationToken cancellationToken);

    public Task<Dictionary<CategoryId, ulong>> GetCategoriesPostsCountAsync(GetCategoriesPostsCountQuery request,
        CancellationToken cancellationToken);

    public Task<Dictionary<CategoryId, T>> GetCategoriesPostsLatestAsync<T>(GetCategoriesPostsLatestQuery<T> request,
        CancellationToken cancellationToken);
}