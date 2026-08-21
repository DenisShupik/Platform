using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;
using UserService.Infrastructure.Grpc.Contracts;

namespace NotificationService.Infrastructure.Clients;

public sealed class UserDirectoryReader(IGrpcUserService userService) : IUserDirectoryReader
{
    public async ValueTask<IReadOnlyList<UserSummaryDto>> GetUsersAsync(
        ICollection<UserId> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0) return [];

        var response = await userService.GetUsersAsync(new GetUsersRequest
        {
            UserIds = new IdSet<UserId, Guid>([.. userIds])
        }, cancellationToken);

        return
        [
            .. response.Users.Select(user => new UserSummaryDto
            {
                UserId = user.UserId,
                Username = user.Username
            })
        ];
    }
}
