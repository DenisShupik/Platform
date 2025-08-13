using UserService.Domain.Errors;
using UserService.Domain.ValueObjects;
using OneOf;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IUserWriteRepository
{
    Task<OneOf<User, UserNotFoundError>> GetOneAsync(UserId userId, CancellationToken cancellationToken);
}