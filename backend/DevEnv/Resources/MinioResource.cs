using Microsoft.Extensions.DependencyInjection;

namespace DevEnv.Resources;

public sealed class MinioResource : ContainerResource, IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";
    internal const string ManagementEndpointName = "management";

    public EndpointReference PrimaryEndpoint { get; }

    public MinioResource(string name) : base(name)
    {
        PrimaryEndpoint = new(this, PrimaryEndpointName);
    }

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}");
}

internal static class MinioContainerImageTags
{
    public const string Registry = "docker.io";
    public const string Image = "minio/minio";
    public const string Tag = "latest";
}

public static class MinioBuilderExtensions
{
    public static IResourceBuilder<MinioResource> AddMinio(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        IResourceBuilder<ParameterResource> username,
        IResourceBuilder<ParameterResource> password,
        string initScriptPath,
        int port = 9000,
        int managementPort = 9001
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        var minio = new MinioResource(name);

        var resourceBuilder = builder
                .AddResource(minio)
                .WithImage(MinioContainerImageTags.Image, MinioContainerImageTags.Tag)
                .WithImageRegistry(MinioContainerImageTags.Registry)
                .WithHttpEndpoint(port: port, targetPort: 9000, name: MinioResource.PrimaryEndpointName)
                .WithHttpEndpoint(port: managementPort, targetPort: 9001, name: MinioResource.ManagementEndpointName)
                .WithEnvironment("MINIO_ADDRESS", ":9000")
                .WithEnvironment("MINIO_CONSOLE_ADDRESS", ":9001")
                .WithEnvironment("MINIO_ROOT_USER", username)
                .WithEnvironment("MINIO_ROOT_PASSWORD", password)
                .WithBindMount(initScriptPath, "/init/init.sh", true)
                .WithEntrypoint("/bin/sh")
                .WithEntrypoint("/init/init.sh")
                .WithArgs("server", "/data")
            ;
        
        var endpoint = resourceBuilder.Resource.GetEndpoint(MinioResource.PrimaryEndpointName);
        var healthCheckKey = $"{name}_check";
        
        builder.Services.AddHealthChecks()
            .AddUrlGroup(options =>
            {
                var uri = new Uri(endpoint.Url);
                options.AddUri(new Uri(uri,"/minio/health/live"), setup => setup.ExpectHttpCode(200));
                options.AddUri(new Uri(uri, "/minio/health/cluster"), setup => setup.ExpectHttpCode(200));
                options.AddUri(new Uri(uri, "/minio/health/cluster/read"), setup => setup.ExpectHttpCode(200));
            }, healthCheckKey);

        resourceBuilder.WithHealthCheck(healthCheckKey);
        
        return resourceBuilder;
    }
}