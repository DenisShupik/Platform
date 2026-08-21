using CoreService.Application.Interfaces;
using Grpc.Core;
using Shared.Domain.ValueObjects;
using UserService.Infrastructure.Grpc.Contracts;

namespace CoreService.Infrastructure.Clients;

public sealed class UserStatusGrpcReader(IGrpcUserService userService) : IUserStatusReader
{
    public async ValueTask<bool> ExistsAsync(UserId userId, CancellationToken cancellationToken) =>
        await GetAsync(userId, cancellationToken) is not null;

    public async ValueTask<bool> IsActiveAsync(UserId userId, CancellationToken cancellationToken) =>
        (await GetAsync(userId, cancellationToken))?.Enabled == true;

    private async ValueTask<GetUserResponse?> GetAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await userService.GetUserAsync(
                new GetUserRequest { UserId = userId },
                cancellationToken);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
