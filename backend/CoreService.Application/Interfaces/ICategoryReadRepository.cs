using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using Shared.Domain.Abstractions.Results;
using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface ICategoryReadRepository
{
    public Task<Result<T, CategoryNotFoundError>> GetOneAsync<T>(GetCategoryQuery<T> query,
        CancellationToken cancellationToken) where T : notnull;

    public Task<Result<AuthorizationScope, CategoryNotFoundError>> GetAuthorizationScopeAsync(
        CategoryId categoryId,
        CancellationToken cancellationToken);

    public Task<Dictionary<CategoryId, Result<T, CategoryNotFoundError>>> GetBulkAsync<T>(
        GetCategoriesBulkQuery<T> query, CancellationToken cancellationToken) where T : notnull;

    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetCategoriesPagedQuery<T> query, CancellationToken cancellationToken);

    public Task<Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>> GetCategoriesThreadsCountAsync(
        GetCategoriesThreadsCountQuery query,
        CancellationToken cancellationToken);

    public Task<Result<IReadOnlyList<T>, CategoryNotFoundError>> GetCategoryThreadsAsync<T>(
        GetCategoryThreadsPagedQuery<T> query, CancellationToken cancellationToken);

    public Task<Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>> GetCategoriesPostsCountAsync(GetCategoriesPostsCountQuery request,
        CancellationToken cancellationToken);

    public Task<Dictionary<CategoryId, T>> GetCategoriesPostsLatestAsync<T>(GetCategoriesPostsLatestQuery<T> query,
        CancellationToken cancellationToken);
}
