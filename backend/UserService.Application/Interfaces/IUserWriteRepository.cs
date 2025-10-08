using UserService.Domain.Errors;
using UserService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IUserWriteRepository
{
    Task<Result<User, UserNotFoundError>> GetOneAsync(UserId userId, CancellationToken cancellationToken);
}