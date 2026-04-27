using Shared.Domain.ValueObjects;
using UserService.Application.Dtos;

namespace UserService.Infrastructure.Cache;

public interface IUserServiceCache
{
    ValueTask<UserDto> GetOrSetAsync(UserId userId, Func<CancellationToken, Task<UserDto>> factory,
        CancellationToken cancellationToken);

    ValueTask RemoveAsync(UserId userId, CancellationToken cancellationToken);
}