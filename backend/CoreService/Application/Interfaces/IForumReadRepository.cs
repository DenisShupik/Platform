using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface IForumReadRepository
{
    public Task<OneOf<T, ForumNotFoundError>> GetOneAsync<T>(ForumId id, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetBulkAsync<T>(List<ForumId> ids, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsQuery request, CancellationToken cancellationToken);

    public Task<Dictionary<ForumId, long>> GetForumsCategoriesCountAsync(GetForumsCategoriesCountQuery request,
        CancellationToken cancellationToken);

    public Task<Dictionary<ForumId, T[]>> GetForumsCategoriesLatestAsync<T>(GetForumsCategoriesLatestQuery request,
        CancellationToken cancellationToken) where T : IHasForumId;
}