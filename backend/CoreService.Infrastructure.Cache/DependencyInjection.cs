using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZiggyCreatures.Caching.Fusion;

namespace CoreService.Infrastructure.Cache;

public static class DependencyInjection
{
    public static void RegisterCoreServiceCache(this IHostApplicationBuilder builder,
        Action<FusionCacheEntryOptions> action)
    {
        builder.Services
            .AddFusionCache(Constants.CacheName)
            .WithCacheKeyPrefixByCacheName()
            .WithDefaultEntryOptions(action)
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        builder.Services.AddSingleton<ICoreServiceCache, CoreServiceCache>();
    }
}