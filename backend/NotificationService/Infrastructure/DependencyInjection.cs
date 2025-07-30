using CoreService.Infrastructure.Grpc.Client;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;
using OpenTelemetry.Trace;
using SharedKernel.Application.Interfaces;
using SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;
using SharedKernel.Infrastructure.Interfaces;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using UserService.Infrastructure.Grpc.Client;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices<T>(this IHostApplicationBuilder builder)
        where T : class, IDbOptions
    {
        builder.Services.RegisterDbContext<ApplicationDbContext, T>(Constants.DatabaseSchema);

        builder.Services.AddTickerQ(options =>
        {
            options.AddOperationalStore<ApplicationDbContext>(efOpt =>
            {
                efOpt.CancelMissedTickersOnApplicationRestart();
            });
            //options.AddDashboard(basePath: "/jobs");
        });

        builder.Services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IThreadSubscriptionReadRepository, ThreadSubscriptionReadRepository>()
            .AddScoped<IThreadSubscriptionRepository, ThreadSubscriptionRepository>()
            .AddScoped<INotificationRepository, NotificationRepository>()
            .AddScoped<IUserNotificationReadRepository, UserNotificationReadRepository>()
            .AddScoped<IUserNotificationRepository, UserNotificationRepository>();
        
        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing.AddEntityFrameworkCoreInstrumentation());

        builder.Services.RegisterFusionCache();
        builder.RegisterCoreServiceGrpcClient();
        builder.RegisterUserServiceGrpcClient();
    }
}