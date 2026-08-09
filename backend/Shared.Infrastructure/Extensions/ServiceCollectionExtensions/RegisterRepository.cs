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
        services.AddScoped<TImplementation>();
        services.AddScoped<TRepository>(provider =>
        {
            var repository = provider.GetRequiredService<TImplementation>();
            var contextAccessor = enableCallDiagnostics
                ? provider.GetService<RepositoryCallContextAccessor>()
                : null;

            return contextAccessor is null
                ? repository
                : RepositoryCallProxy<TRepository>.Create(repository, contextAccessor);
        });

        return services;
    }
}
