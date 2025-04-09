using SharedKernel.Domain.ValueObjects;
using UserService.Domain.Errors;
using OneOf;
using UserService.Application.UseCases;

namespace UserService.Application.Interfaces;

public interface IUserReadRepository
{
    public Task<OneOf<T, UserNotFoundError>> GetByIdAsync<T>(UserId userId, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetByIdsAsync<T>(List<UserId> userId, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetUsersQuery query, CancellationToken cancellationToken);
}