using Grpc.Core;
using Shared.Domain.Errors;

namespace UserService.Infrastructure.Grpc.Contracts;

public static class GrpcErrorHelper
{
    public static RpcException GetRpcException(this UserNotFoundError error)
    {
        var metadata = new Metadata
        {
            { "error-type", nameof(UserNotFoundError) }
        };

        return new RpcException(new Status(StatusCode.NotFound, "User not found"), metadata);
    }
}