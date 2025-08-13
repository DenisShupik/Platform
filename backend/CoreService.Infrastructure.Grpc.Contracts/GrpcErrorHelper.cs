using CoreService.Domain.Errors;
using Grpc.Core;

namespace CoreService.Infrastructure.Grpc.Contracts;

public static class GrpcErrorHelper
{
    public static RpcException GetRpcException(this ThreadNotFoundError error)
    {
        var metadata = new Metadata
        {
            { "error-type", nameof(ThreadNotFoundError) },
            { "thread-id", error.ThreadId.ToString() }
        };

        return new RpcException(new Status(StatusCode.NotFound, "Thread not found"), metadata);
    }

    public static RpcException GetRpcException(this NonThreadOwnerError error)
    {
        var metadata = new Metadata
        {
            { "error-type", nameof(NonThreadOwnerError) },
            { "thread-id", error.ThreadId.ToString() },
        };

        return new RpcException(new Status(StatusCode.PermissionDenied, "Non thread owner"), metadata);
    }

    public static RpcException GetRpcException(this PostNotFoundError error)
    {
        var metadata = new Metadata
        {
            { "error-type", nameof(PostNotFoundError) },
            { "post-id", error.PostId.ToString() }
        };

        return new RpcException(new Status(StatusCode.NotFound, "Post not found"), metadata);
    }
}