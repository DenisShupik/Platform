using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace UserService.Infrastructure.Cache;

public static class DependencyInjection
{
    public static void RegisterUserServiceCache(this IServiceCollection services,
        Action<FusionCacheEntryOptions> action)
    {
        services
            .AddFusionCache(Constants.CacheName)
            .WithCacheKeyPrefixByCacheName()
            .WithDefaultEntryOptions(action)
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        services.AddSingleton<IUserServiceCache, UserServiceCache>();
    }
}