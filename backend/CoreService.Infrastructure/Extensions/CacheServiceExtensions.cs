using Microsoft.Extensions.DependencyInjection;
using CoreService.Application.Interfaces;
using CoreService.Infrastructure.Services;
using ZiggyCreatures.Caching.Fusion;

namespace CoreService.Infrastructure.Extensions;

public static class CacheServiceExtensions
{
    public static IServiceCollection RegisterCategoryStatsCache(this IServiceCollection services)
    {
        services.AddFusionCache(nameof(CategoryStatsCache))
            .WithDefaultEntryOptions(opt => { opt.Duration = TimeSpan.MaxValue; })
            .WithCacheKeyPrefix()
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        services.AddSingleton<ICategoryStatsCache, CategoryStatsCache>();

        return services;
    }
}