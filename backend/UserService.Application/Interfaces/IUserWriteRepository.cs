using UserService.Domain.Errors;
using UserService.Domain.ValueObjects;
using OneOf;

namespace UserService.Application.Interfaces;

public interface IUserWriteRepository
{
    Task<OneOf<T, UserNotFoundError>> GetByIdAsync<T>(UserId userId, CancellationToken cancellationToken);
}