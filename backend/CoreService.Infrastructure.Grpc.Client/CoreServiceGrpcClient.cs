using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Grpc.Contracts;
using Mapster;
using ZiggyCreatures.Caching.Fusion;

namespace CoreService.Infrastructure.Grpc.Client;

public sealed class CoreServiceGrpcClient : ICoreServiceClient
{
    private readonly IFusionCache _cache;
    private readonly IGrpcCoreService _grpcClient;

    public CoreServiceGrpcClient(IFusionCacheProvider cacheProvider, IGrpcCoreService grpcClient)
    {
        _cache = cacheProvider.GetCache(Constants.CacheName);
        _grpcClient = grpcClient;
    }

    public async ValueTask<ThreadDto> GetThreadAsync(ThreadId threadId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync<ThreadDto>(
            $"thread:{threadId}",
            async ct =>
            {
                var response = await _grpcClient.GetThreadAsync(
                    new GetThreadRequest { ThreadId = threadId.Value }, ct);
                return response.Adapt<ThreadDto>();
            }, token: cancellationToken);
    }

    public async ValueTask<PostDto> GetPostAsync(ThreadId threadId, PostId postId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync<PostDto>(
            $"post:{threadId}:{postId}",
            async ct =>
            {
                var response = await _grpcClient.GetPostAsync(
                    new GetPostRequest { ThreadId = threadId.Value, PostId = postId.Value }, ct);
                return response.Adapt<PostDto>();
            }, token: cancellationToken);
    }
}