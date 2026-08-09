using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Diagnostics;

namespace Shared.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepository<TRepository, TImplementation>(
        this IServiceCollection services,
        bool enableCallDiagnostics = true)
        where TRepository : class
        where TImplementation : class, TRepository
    {
        var diagnosticsEnabled = enableCallDiagnostics && services.Any(descriptor =>
            descriptor.ServiceType == typeof(RepositoryCallContextAccessor));

        if (!diagnosticsEnabled)
        {
            services.AddScoped<TRepository, TImplementation>();
            return services;
        }

        services.AddScoped<TImplementation>();
        services.AddScoped<TRepository>(provider =>
        {
            var repository = provider.GetRequiredService<TImplementation>();
            var contextAccessor = provider.GetRequiredService<RepositoryCallContextAccessor>();
            return RepositoryCallProxy<TRepository>.Create(repository, contextAccessor);
        });

        return services;
    }
}
