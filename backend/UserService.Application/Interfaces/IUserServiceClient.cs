using Shared.Domain.ValueObjects;
using UserService.Application.Dtos;

namespace UserService.Application.Interfaces;

public interface IUserServiceClient
{
    ValueTask<UserDto> GetUserAsync(UserId userId, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<UserDto>> GetUsersAsync(ICollection<UserId> userIds,
        CancellationToken cancellationToken);
}