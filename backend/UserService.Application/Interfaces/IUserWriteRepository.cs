using Shared.Domain.Abstractions.Results;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IUserWriteRepository
{
    Task<Result<User, UserNotFoundError>> GetOneAsync(UserId userId, CancellationToken cancellationToken);
}