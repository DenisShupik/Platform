using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Grpc.Contracts;
using ZiggyCreatures.Caching.Fusion;

namespace UserService.Infrastructure.Grpc.Client;

public sealed class UserServiceGrpcClient
{
    private readonly IFusionCache _cache;
    private readonly IGrpcUserService _grpcClient;

    public UserServiceGrpcClient(IFusionCacheProvider cacheProvider, IGrpcUserService grpcClient)
    {
        _cache = cacheProvider.GetCache(Constants.CacheName);
        _grpcClient = grpcClient;
    }

    public async ValueTask<GetUserResponse> GetUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync<GetUserResponse>(
            $"user:{userId}",
            async ct =>
            {
                var response = await _grpcClient.GetUserAsync(
                    new GetUserRequest { UserId = userId.Value }, ct);
                return response;
            }, token: cancellationToken);
    }
}