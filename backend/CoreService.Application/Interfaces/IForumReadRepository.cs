using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface IForumReadRepository
{
    public Task<Result<T, ForumNotFoundError, PolicyViolationError, ReadPolicyRestrictedError>> GetOneAsync<T>(
        GetForumQuery<T> query, CancellationToken cancellationToken) where T : notnull;

    public Task<Dictionary<ForumId,Result<T, ForumNotFoundError, PolicyViolationError, ReadPolicyRestrictedError>>> GetBulkAsync<T>(
        GetForumsBulkQuery<T> query, CancellationToken cancellationToken) where T : notnull;

    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsPagedQuery<T> query, CancellationToken cancellationToken);
    public Task<ulong> GetCountAsync(GetForumsCountQuery request, CancellationToken cancellationToken);

    public Task<Dictionary<ForumId, ulong>> GetForumsCategoriesCountAsync(GetForumsCategoriesCountQuery query,
        CancellationToken cancellationToken);
}