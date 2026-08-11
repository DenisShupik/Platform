using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Presentation.Extensions;

public static class ServiceHealthCheckExtensions
{
    private const string HealthPath = "/health";
    public const string LivenessPath = $"{HealthPath}/live";
    public const string ReadinessPath = $"{HealthPath}/ready";

    public static IServiceCollection AddServiceHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();

        return services;
    }

    public static IEndpointRouteBuilder MapServiceHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(LivenessPath, new HealthCheckOptions
        {
            Predicate = static _ => false
        });
        endpoints.MapHealthChecks(ReadinessPath);

        return endpoints;
    }
}
