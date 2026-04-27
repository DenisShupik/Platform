using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace UserService.Infrastructure.Grpc.Contracts;

[Service]
public interface IGrpcUserService
{
    [Operation]
    ValueTask<GetUserResponse> GetUserAsync(GetUserRequest request, CallContext context = default);
    
    [Operation]
    ValueTask<GetUsersResponse> GetUsersAsync(GetUsersRequest request, CallContext context = default);
}