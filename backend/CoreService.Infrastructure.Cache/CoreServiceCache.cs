using System.Runtime.CompilerServices;
using CoreService.Application.Dtos;
using CoreService.Domain.ValueObjects;
using ZiggyCreatures.Caching.Fusion;

namespace CoreService.Infrastructure.Cache;

public sealed class CoreServiceCache : ICoreServiceCache
{
    private readonly IFusionCache _cache;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetThreadKey(ThreadId threadId) => $"thread:{threadId}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetPostKey(ThreadId threadId, PostId postId) => $"post:{threadId}:{postId}";

    public CoreServiceCache(IFusionCacheProvider cacheProvider)
    {
        _cache = cacheProvider.GetCache(Constants.CacheName);
    }

    public ValueTask<ThreadDto> GetOrSetAsync(ThreadId threadId, Func<CancellationToken, Task<ThreadDto>> factory,
        CancellationToken cancellationToken) =>
        _cache.GetOrSetAsync<ThreadDto>(GetThreadKey(threadId), factory, token: cancellationToken);

    public ValueTask<PostDto> GetOrSetAsync(ThreadId threadId, PostId postId,
        Func<CancellationToken, Task<PostDto>> factory, CancellationToken cancellationToken) =>
        _cache.GetOrSetAsync<PostDto>(GetPostKey(threadId, postId), factory, token: cancellationToken);
}