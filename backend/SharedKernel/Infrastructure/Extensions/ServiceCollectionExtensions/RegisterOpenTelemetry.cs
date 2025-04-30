using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static OpenTelemetryBuilder RegisterOpenTelemetry(
        this IServiceCollection services,
        string serviceName
    ) =>
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            })
            .ConfigureResource(resource => { resource.AddService(serviceName); });
}