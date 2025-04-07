using System.Net.Mime;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;
using SharedKernel.Interfaces;
using SharedKernel.Options;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Consumers;
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

        builder.Services.RegisterPooledDbContextFactory<ApplicationDbContext, T>(Constants.DatabaseSchema);

        builder.Services.AddScoped<ApplicationDbContext>(serviceProvider => serviceProvider
            .GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
            .CreateDbContext()
        );

        builder.Services
            .AddScoped<IUserReadRepository, UserReadRepository>();

        builder.Services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<UserEventConsumer>();

            configurator.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var keycloakOptions = context.GetRequiredService<IOptions<KeycloakOptions>>().Value;
                cfg.Host(host: rabbitMqOptions.Host, port: rabbitMqOptions.Port,
                    virtualHost: rabbitMqOptions.VirtualHost, h =>
                    {
                        h.Username(rabbitMqOptions.Username);
                        h.Password(rabbitMqOptions.Password);
                    });

                cfg.ReceiveEndpoint($"{nameof(UserService)}Queue", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.DefaultContentType = new ContentType("application/json");
                    e.UseRawJsonDeserializer();
                    e.Bind("amq.topic", ex =>
                    {
                        ex.ExchangeType = ExchangeType.Topic;
                        ex.RoutingKey = $"KK.EVENT.*.{keycloakOptions.Realm}.#";
                    });
                    e.ConfigureConsumer<UserEventConsumer>(context);
                });
            });
        });
    }
}