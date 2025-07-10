using Grpc.Core;
using UserService.Domain.Errors;

namespace UserService.Infrastructure.Grpc.Contracts;

public static class GrpcErrorHelper
{
    public static RpcException GetRpcException(this UserNotFoundError error)
    {
        var metadata = new Metadata
        {
            { "error-type", nameof(UserNotFoundError) },
            { "user-id", error.UserId.ToString() }
        };

        return new RpcException(new Status(StatusCode.NotFound, "User not found"), metadata);
    }
}