using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface IForumReadRepository
{
    public Task<Result<T, ForumNotFoundError>> GetOneAsync<T>(ForumId id, CancellationToken cancellationToken) where T : notnull;
    public Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<ForumId, Guid> ids, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsPagedQuery<T> request, CancellationToken cancellationToken);
    public Task<ulong> GetCountAsync(GetForumsCountQuery request, CancellationToken cancellationToken);

    public Task<Dictionary<ForumId, ulong>> GetForumsCategoriesCountAsync(GetForumsCategoriesCountQuery request,
        CancellationToken cancellationToken);
}