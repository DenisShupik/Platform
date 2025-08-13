using System.Runtime.CompilerServices;
using UserService.Application.Dtos;
using UserService.Domain.ValueObjects;
using ZiggyCreatures.Caching.Fusion;

namespace UserService.Infrastructure.Cache;

public sealed class UserServiceCache : IUserServiceCache
{
    private readonly IFusionCache _cache;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetUserKey(UserId userId) => $"user:{userId}";

    public UserServiceCache(IFusionCacheProvider cacheProvider)
    {
        _cache = cacheProvider.GetCache(Constants.CacheName);
    }

    public ValueTask<UserDto> GetOrSetAsync(UserId userId, Func<CancellationToken, Task<UserDto>> factory,
        CancellationToken cancellationToken) =>
        _cache.GetOrSetAsync<UserDto>(GetUserKey(userId), factory, token: cancellationToken);

    public ValueTask RemoveAsync(UserId userId, CancellationToken cancellationToken) =>
        _cache.RemoveAsync(GetUserKey(userId), token: cancellationToken);
}