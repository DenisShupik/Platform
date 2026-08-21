using CoreService.Infrastructure.Grpc.Contracts;

namespace CoreService.Infrastructure.Grpc.Client;

/// <summary>
/// Low-level client for the internal CoreService gRPC contract.
/// Business-specific ports and response mapping belong to the consuming service.
/// </summary>
public sealed class CoreServiceGrpcClient(IGrpcCoreService client)
{
    public ValueTask<GetThreadResponse> GetThreadAsync(
        GetThreadRequest request,
        CancellationToken cancellationToken = default) =>
        client.GetThreadAsync(request, cancellationToken);

    public ValueTask<GetThreadsResponse> GetThreadsAsync(
        GetThreadsRequest request,
        CancellationToken cancellationToken = default) =>
        client.GetThreadsAsync(request, cancellationToken);

    public ValueTask<GetPostResponse> GetPostAsync(
        GetPostRequest request,
        CancellationToken cancellationToken = default) =>
        client.GetPostAsync(request, cancellationToken);

    public ValueTask<GrantInitialPlatformAdministratorCapabilitiesResponse>
        GrantInitialPlatformAdministratorCapabilitiesAsync(
            GrantInitialPlatformAdministratorCapabilitiesRequest request,
            CancellationToken cancellationToken = default) =>
        client.GrantInitialPlatformAdministratorCapabilitiesAsync(request, cancellationToken);
}
