using UserService.Domain.Errors;
using OneOf;
using Shared.Domain.Abstractions;
using UserService.Application.UseCases;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Interfaces;

public interface IUserReadRepository
{
    public Task<OneOf<T, UserNotFoundError>> GetOneAsync<T>(UserId userId, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<UserId, Guid> userIds, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetUsersPagedQuery<T> pagedQuery, CancellationToken cancellationToken);
}