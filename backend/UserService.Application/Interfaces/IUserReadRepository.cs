using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;
using UserService.Application.UseCases;

namespace UserService.Application.Interfaces;

public interface IUserReadRepository
{
    public Task<Result<T, UserNotFoundError>> GetOneAsync<T>(UserId userId, CancellationToken cancellationToken)
        where T : notnull;

    public Task<Dictionary<UserId, Result<T, UserNotFoundError>>> GetBulkAsync<T>(IdSet<UserId, Guid> userIds, CancellationToken cancellationToken)
        where T : notnull;
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetUsersPagedQuery<T> pagedQuery, CancellationToken cancellationToken);
}