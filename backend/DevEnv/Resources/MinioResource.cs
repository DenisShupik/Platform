namespace DevEnv.Resources;

public sealed class MinioResource : ContainerResource, IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "tcp";
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
        int port = 9000,
        int managementPort = 9001
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        var minio = new MinioResource(name);

        return builder.AddResource(minio)
                .WithEndpoint(port: port, targetPort: 9000, name: MinioResource.PrimaryEndpointName)
                .WithHttpEndpoint(port: managementPort, targetPort: 9001, name: MinioResource.ManagementEndpointName)
                .WithImage(MinioContainerImageTags.Image, MinioContainerImageTags.Tag)
                .WithImageRegistry(MinioContainerImageTags.Registry)
                .WithEnvironment("MINIO_ADDRESS", ":9000")
                .WithEnvironment("MINIO_CONSOLE_ADDRESS", ":9001")
                .WithEnvironment("MINIO_ROOT_USER", username)
                .WithEnvironment("MINIO_ROOT_PASSWORD", password)
                .WithBindMount(".config/minio.sh","/init/init.sh",true)
                .WithEntrypoint("/bin/sh")
                .WithEntrypoint("/init/init.sh")
                .WithArgs("server", "/data")
            ;
    }
}