using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZiggyCreatures.Caching.Fusion;

namespace UserService.Infrastructure.Cache;

public static class DependencyInjection
{
    public static void RegisterUserServiceCache(this IHostApplicationBuilder builder,
        Action<FusionCacheEntryOptions> action)
    {
        builder.Services
            .AddFusionCache(Constants.CacheName)
            .WithCacheKeyPrefixByCacheName()
            .WithDefaultEntryOptions(action)
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        builder.Services.AddSingleton<IUserServiceCache, UserServiceCache>();
    }
}