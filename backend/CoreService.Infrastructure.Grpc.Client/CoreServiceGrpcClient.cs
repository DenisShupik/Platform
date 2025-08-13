using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Cache;
using CoreService.Infrastructure.Grpc.Contracts;
using Mapster;
using SharedKernel.Infrastructure.Abstractions;

namespace CoreService.Infrastructure.Grpc.Client;

public sealed class CoreServiceGrpcClient : ICoreServiceClient
{
    
    private sealed class CoreDataLoader : DataLoader<ThreadId, ThreadDto>
    {
        private readonly IGrpcCoreService _grpcClient;

        public CoreDataLoader(IGrpcCoreService grpcClient, int maxBatchSize, TimeSpan maxWaitTime) : base(
            maxBatchSize, maxWaitTime)
        {
            _grpcClient = grpcClient;
        }

        protected override async Task<Dictionary<ThreadId, ThreadDto>> FetchAsync(IReadOnlyList<ThreadId> keys,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _grpcClient.GetThreadsAsync(
                    new GetThreadsRequest { ThreadIds = keys.ToHashSet() }, cancellationToken);
                return response.Threads.Select(user => user.Adapt<ThreadDto>()).ToDictionary(threadDto => threadDto.ThreadId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
    
    private readonly ICoreServiceCache _cache;
    private readonly IGrpcCoreService _grpcClient;
    private readonly CoreDataLoader _dataLoader;

    public CoreServiceGrpcClient(ICoreServiceCache cache, IGrpcCoreService grpcClient)
    {
        _cache = cache;
        _grpcClient = grpcClient;
        _dataLoader = new CoreDataLoader(_grpcClient, 100, TimeSpan.FromMilliseconds(25));
    }

    public async ValueTask<ThreadDto> GetThreadAsync(ThreadId threadId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            threadId,
            async ct =>
            {
                var response = await _grpcClient.GetThreadAsync(
                    new GetThreadRequest { ThreadId = threadId }, ct);
                return response.Adapt<ThreadDto>();
            }, cancellationToken);
    }
    
    public async ValueTask<IReadOnlyList<ThreadDto>> GetThreadsAsync(ISet<ThreadId> userIds,
        CancellationToken cancellationToken)
    {
        var tasks = userIds
            .Select(userId =>
                _cache.GetOrSetAsync(userId, _ => _dataLoader.LoadAsync(userId), cancellationToken).AsTask());
        return await Task.WhenAll(tasks);
    }

    public async ValueTask<PostDto> GetPostAsync(ThreadId threadId, PostId postId, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            threadId, postId,
            async ct =>
            {
                var response = await _grpcClient.GetPostAsync(
                    new GetPostRequest { ThreadId = threadId, PostId = postId }, ct);
                return response.Adapt<PostDto>();
            }, cancellationToken);
    }
}