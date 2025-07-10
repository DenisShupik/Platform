using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Grpc.Contracts;
using ZiggyCreatures.Caching.Fusion;

namespace CoreService.Infrastructure.Grpc.Client;

public sealed class CoreServiceGrpcClient
{
    private readonly IFusionCache _cache;
    private readonly IGrpcCoreService _grpcClient;

    public CoreServiceGrpcClient(IFusionCacheProvider cacheProvider, IGrpcCoreService grpcClient)
    {
        _cache = cacheProvider.GetCache(Constants.CacheName);
        _grpcClient = grpcClient;
    }

    public async ValueTask<GetThreadResponse> GetThreadAsync(ThreadId threadId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync<GetThreadResponse>(
            $"thread:{threadId}",
            async ct =>
            {
                var response = await _grpcClient.GetThreadAsync(
                    new GetThreadRequest { ThreadId = threadId.Value }, ct);
                return response;
            }, token: cancellationToken);
    }

    public async ValueTask<GetPostResponse> GetPostAsync(ThreadId threadId, PostId postId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync<GetPostResponse>(
            $"post:{threadId}:{postId}",
            async ct =>
            {
                var response = await _grpcClient.GetPostAsync(
                    new GetPostRequest { ThreadId = threadId.Value, PostId = postId.Value }, ct);
                return response;
            }, token: cancellationToken);
    }
}