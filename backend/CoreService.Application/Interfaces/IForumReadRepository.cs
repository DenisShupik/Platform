using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using OneOf;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.Interfaces;

public interface IForumReadRepository
{
    public Task<OneOf<T, ForumNotFoundError>> GetOneAsync<T>(ForumId id, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<ForumId> ids, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsPagedQuery request, CancellationToken cancellationToken);
    public Task<long> GetCountAsync(GetForumsCountQuery request, CancellationToken cancellationToken);

    public Task<Dictionary<ForumId, long>> GetForumsCategoriesCountAsync(GetForumsCategoriesCountQuery request,
        CancellationToken cancellationToken);
}