using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace CoreService.Infrastructure.Cache;

public static class DependencyInjection
{
    public static void RegisterCoreServiceCache(this IServiceCollection services,
        Action<FusionCacheEntryOptions> action)
    {
        services
            .AddFusionCache(Constants.CacheName)
            .WithCacheKeyPrefixByCacheName()
            .WithDefaultEntryOptions(action)
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        services.AddSingleton<ICoreServiceCache, CoreServiceCache>();
    }
}