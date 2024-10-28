using Microsoft.EntityFrameworkCore;
using TopicService.Application.Interfaces;
using TopicService.Infrastructure.Persistence;
using ZiggyCreatures.Caching.Fusion;

namespace TopicService.Infrastructure.Services;

public sealed class CategoryStatsCache : ICategoryStatsCache
{
    private readonly IFusionCache _cache;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public CategoryStatsCache(IFusionCacheProvider cacheProvider,
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _cache = cacheProvider.GetCache(nameof(CategoryStatsCache));
        _dbContextFactory = dbContextFactory;
    }

    public async ValueTask<long> GetTopicCountAsync(long categoryId, CancellationToken cancellationToken)
    {
        var topicCount = await _cache.GetOrSetAsync($"TopicCount:{categoryId}", async ct =>
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
            return await context.Topics.LongCountAsync(t => t.CategoryId == categoryId, ct);
        }, token: cancellationToken);

        return topicCount;
    }

    public async ValueTask UpdateTopicCountAsync(long categoryId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var count = await context.Topics.LongCountAsync(t => t.CategoryId == categoryId);
        await _cache.SetAsync($"TopicCount:{categoryId}", count);
    }
}