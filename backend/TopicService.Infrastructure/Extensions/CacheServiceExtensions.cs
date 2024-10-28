using Microsoft.Extensions.DependencyInjection;
using TopicService.Application.Interfaces;
using TopicService.Infrastructure.Services;
using ZiggyCreatures.Caching.Fusion;

namespace TopicService.Infrastructure.Extensions;

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