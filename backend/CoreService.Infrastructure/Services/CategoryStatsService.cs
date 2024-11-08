using Microsoft.EntityFrameworkCore;
using CoreService.Application.Interfaces;
using CoreService.Infrastructure.Persistence;
using ZiggyCreatures.Caching.Fusion;

namespace CoreService.Infrastructure.Services;

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

    public async ValueTask<long> GetThreadCountAsync(long categoryId, CancellationToken cancellationToken)
    {
        var threadCount = await _cache.GetOrSetAsync($"ThreadCount:{categoryId}", async ct =>
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(ct);
            return await context.Threads.LongCountAsync(t => t.CategoryId == categoryId, ct);
        }, token: cancellationToken);

        return threadCount;
    }

    public async ValueTask UpdateThreadCountAsync(long categoryId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var count = await context.Threads.LongCountAsync(t => t.CategoryId == categoryId);
        await _cache.SetAsync($"ThreadCount:{categoryId}", count);
    }
}