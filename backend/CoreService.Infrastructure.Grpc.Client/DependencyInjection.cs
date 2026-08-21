using CoreService.Infrastructure.Grpc.Contracts;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.ClientFactory;

namespace CoreService.Infrastructure.Grpc.Client;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the low-level client for CoreService's internal gRPC endpoint.
    /// Authentication, resilience and service discovery handlers can be added to the returned builder.
    /// </summary>
    public static IHttpClientBuilder AddCoreServiceGrpcClient(this IServiceCollection services, Uri address)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(address);

        if (!address.IsAbsoluteUri)
            throw new ArgumentException("CoreService gRPC address must be absolute.", nameof(address));

        var builder = services.AddCodeFirstGrpcClient<IGrpcCoreService>(options => options.Address = address);
        services.AddSingleton<CoreServiceGrpcClient>();
        return builder;
    }
}
