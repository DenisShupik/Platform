using OpenTelemetry.Trace;
using ProtoBuf.Grpc.Server;
using SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;
using SharedKernel.Infrastructure.Interfaces;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;
using SharedKernel.Presentation.Options;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Persistence.Repositories;
using Constants = UserService.Infrastructure.Persistence.Constants;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices<T>(this IHostApplicationBuilder builder)
        where T : class, IDbOptions
    {
        builder.Services.RegisterOptions<RabbitMqOptions, RabbitMqOptionsValidator>(builder.Configuration);

        builder.Services.RegisterDbContext<ApplicationDbContext, T>(Constants.DatabaseSchema);

        builder.Services
            .AddScoped<IUserReadRepository, UserReadRepository>();

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing
                .AddEntityFrameworkCoreInstrumentation()
            );

        builder.Services.AddCodeFirstGrpc();
        builder.Services.AddCodeFirstGrpcReflection();
    }
}