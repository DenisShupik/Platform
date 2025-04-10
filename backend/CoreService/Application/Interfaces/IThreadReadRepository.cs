using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface IThreadReadRepository
{
    public Task<OneOf<T, ThreadNotFoundError>> GetByIdAsync<T>(ThreadId id, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetByIdsAsync<T>(List<ThreadId> ids, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetCategoryThreadsAsync<T>(GetCategoryThreadsQuery request, CancellationToken cancellationToken);
}