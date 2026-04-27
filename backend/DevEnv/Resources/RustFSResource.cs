using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace DevEnv.Resources;

public sealed class RustFsResource : ContainerResource, IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";
    internal const string WebConsoleEndpointName = "web-console";

    public EndpointReference PrimaryEndpoint { get; }

    public RustFsResource(string name) : base(name)
    {
        PrimaryEndpoint = new(this, PrimaryEndpointName);
    }

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}");
}

internal static class RustFsContainerImageTags
{
    public const string Registry = "docker.io";
    public const string Image = "rustfs/rustfs";
    public const string Tag = "latest";
}

public static class MinioBuilderExtensions
{
    public static IResourceBuilder<RustFsResource> AddRustFs(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        IResourceBuilder<ParameterResource> username,
        IResourceBuilder<ParameterResource> password,
        int port = 9000,
        int managementPort = 9001
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        var minio = new RustFsResource(name);

        var resourceBuilder = builder
                .AddResource(minio)
                .WithImage(RustFsContainerImageTags.Image, RustFsContainerImageTags.Tag)
                .WithImageRegistry(RustFsContainerImageTags.Registry)
                .WithHttpEndpoint(port: port, targetPort: 9000, name: RustFsResource.PrimaryEndpointName)
                .WithHttpEndpoint(port: managementPort, targetPort: 9001, name: RustFsResource.WebConsoleEndpointName)
                .WithEnvironment("RUSTFS_ADDRESS", ":9000")
                .WithEnvironment("RUSTFS_CONSOLE_ENABLE", "true")
                .WithEnvironment("RUSTFS_ACCESS_KEY", username)
                .WithEnvironment("RUSTFS_SECRET_KEY", password)
            ;

        // var endpoint = resourceBuilder.Resource.GetEndpoint(RustFsResource.PrimaryEndpointName);
        // var healthCheckKey = $"{name}_check";
        //
        // builder.Services.AddHealthChecks()
        //     .AddUrlGroup(options =>
        //     {
        //         var uri = new Uri(endpoint.Url);
        //         options.AddUri(new Uri(uri,"/minio/health/live"), setup => setup.ExpectHttpCode(200));
        //         options.AddUri(new Uri(uri, "/minio/health/cluster"), setup => setup.ExpectHttpCode(200));
        //         options.AddUri(new Uri(uri, "/minio/health/cluster/read"), setup => setup.ExpectHttpCode(200));
        //     }, healthCheckKey);
        //
        // resourceBuilder.WithHealthCheck(healthCheckKey);

        return resourceBuilder;
    }
}