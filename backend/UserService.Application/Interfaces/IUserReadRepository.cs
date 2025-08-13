using UserService.Domain.Errors;
using OneOf;
using UserService.Application.UseCases;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Interfaces;

public interface IUserReadRepository
{
    public Task<OneOf<T, UserNotFoundError>> GetOneAsync<T>(UserId userId, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetBulkAsync<T>(HashSet<UserId> userIds, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetUsersPagedQuery pagedQuery, CancellationToken cancellationToken);
}