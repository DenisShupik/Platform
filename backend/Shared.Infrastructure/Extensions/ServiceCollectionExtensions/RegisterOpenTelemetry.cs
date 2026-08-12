using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Shared.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    private const string AspNetCoreActivitySourceName = "Microsoft.AspNetCore";

    public static OpenTelemetryBuilder RegisterOpenTelemetry(
        this IServiceCollection services,
        string serviceName
    ) =>
        services.AddOpenTelemetry()
            .ConfigureResource(resource => { resource.AddService(serviceName); })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(AspNetCoreActivitySourceName)
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            })
            .WithLogging(logging => logging.AddOtlpExporter());
}
