using OpenTelemetry.Trace;
using SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;

namespace FileService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing.AddAWSInstrumentation())
            ;
    }
}