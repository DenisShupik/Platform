using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface IForumReadRepository
{
    public Task<OneOf<T, ForumNotFoundError>> GetByIdAsync<T>(ForumId id, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetByIdsAsync<T>(List<ForumId> ids, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsQuery request, CancellationToken cancellationToken);
}