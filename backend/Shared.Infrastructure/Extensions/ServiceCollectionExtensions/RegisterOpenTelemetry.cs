using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Shared.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static OpenTelemetryBuilder RegisterOpenTelemetry(
        this IServiceCollection services,
        string serviceName
    ) =>
        services.AddOpenTelemetry()
            .ConfigureResource(resource => { resource.AddService(serviceName); })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            })
            .WithLogging(logging => logging.AddOtlpExporter());
}