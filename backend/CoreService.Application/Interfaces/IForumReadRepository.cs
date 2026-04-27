using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IForumReadRepository
{
    Task<Result<T, ForumNotFoundError>> GetOneAsync<T>(GetForumQuery<T> query,
        CancellationToken cancellationToken) where T : notnull;

    Task<Dictionary<ForumId, Result<T, ForumNotFoundError>>> GetBulkAsync<T>(
        GetForumsBulkQuery<T> query, CancellationToken cancellationToken) where T : notnull;

    Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsPagedQuery<T> query, CancellationToken cancellationToken);
    Task<Count> GetCountAsync(GetForumsCountQuery request, CancellationToken cancellationToken);

    Task<Dictionary<ForumId, Result<Count, ForumNotFoundError>>> GetForumsCategoriesCountAsync(
        GetForumsCategoriesCountQuery query, CancellationToken cancellationToken);
}