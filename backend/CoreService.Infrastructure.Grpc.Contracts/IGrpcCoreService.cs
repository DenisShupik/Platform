using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace CoreService.Infrastructure.Grpc.Contracts;

[Service]
public interface IGrpcCoreService
{
    [Operation]
    ValueTask<GetThreadResponse> GetThreadAsync(GetThreadRequest request, CallContext context = default);

    [Operation]
    ValueTask<GetThreadsResponse> GetThreadsAsync(GetThreadsRequest request, CallContext context = default);

    [Operation]
    ValueTask<GetPostResponse> GetPostAsync(GetPostRequest request, CallContext context = default);
}