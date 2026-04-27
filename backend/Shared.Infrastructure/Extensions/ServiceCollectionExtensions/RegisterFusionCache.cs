using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Options;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;

namespace Shared.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterFusionCache(this IServiceCollection services)
    {
        services.AddFusionCacheSystemTextJsonSerializer();
        services
            .AddOptions<RedisBackplaneOptions>()
            .Configure<IOptions<ValkeyOptions>>((options, redisOptions) =>
            {
                options.Configuration = redisOptions.Value.ConnectionString;
            });
        services
            .AddOptions<RedisCacheOptions>()
            .Configure<IOptions<ValkeyOptions>>((options, redisOptions) =>
            {
                options.Configuration = redisOptions.Value.ConnectionString;
            });
        services.AddStackExchangeRedisCache(_ => { });
        services.AddFusionCacheStackExchangeRedisBackplane();
        return services;
    }
}