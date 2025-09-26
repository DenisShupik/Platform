using ApiGateway.Infrastructure.Interfaces;
using ApiGateway.Infrastructure.Services;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Options;
using ZiggyCreatures.Caching.Fusion;

namespace ApiGateway.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .RegisterOptions<KeycloakOptions, KeycloakOptionsValidator>(builder.Configuration)
            .RegisterOptions<ValkeyOptions, ValkeyOptionsValidator>(builder.Configuration);
        
        var configuration = builder.Configuration.GetRequiredSection("ReverseProxy");

        builder.Services
            .AddReverseProxy()
            .LoadFromConfig(configuration);

        builder.Services.RegisterFusionCache();

        builder.Services
            .AddFusionCache(Constants.CacheName)
            .WithCacheKeyPrefixByCacheName()
            .WithDefaultEntryOptions(options =>
            {
                options.Duration = TimeSpan.FromMinutes(30);
                options.DistributedCacheDuration = TimeSpan.FromHours(1);
            })
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName);

        builder.Services.AddSingleton<IOpenApiAggregatorService, OpenApiAggregatorService>();
    }
}