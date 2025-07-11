using Mapster;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Grpc.Contracts;
using ZiggyCreatures.Caching.Fusion;

namespace UserService.Infrastructure.Grpc.Client;

public sealed class UserServiceGrpcClient : IUserServiceClient
{
    private readonly IFusionCache _cache;
    private readonly IGrpcUserService _grpcClient;

    public UserServiceGrpcClient(IFusionCacheProvider cacheProvider, IGrpcUserService grpcClient)
    {
        _cache = cacheProvider.GetCache(Constants.CacheName);
        _grpcClient = grpcClient;
    }

    public async ValueTask<UserDto> GetUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync<UserDto>(
            $"user:{userId}",
            async ct =>
            {
                var response = await _grpcClient.GetUserAsync(
                    new GetUserRequest { UserId = userId.Value }, ct);
                return response.Adapt<UserDto>();
            }, token: cancellationToken);
    }
}