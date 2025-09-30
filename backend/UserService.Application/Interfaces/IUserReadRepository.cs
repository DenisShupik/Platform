using UserService.Domain.Errors;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Application.UseCases;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Interfaces;

public interface IUserReadRepository
{
    public Task<Result<T, UserNotFoundError>> GetOneAsync<T>(UserId userId, CancellationToken cancellationToken)
        where T : notnull;

    public Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<UserId, Guid> userIds, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetUsersPagedQuery<T> pagedQuery, CancellationToken cancellationToken);
}