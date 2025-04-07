using System.Net.Mime;
using SharedKernel.Options;
using MassTransit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;
using UserService.Presentation.Consumers;

namespace UserService.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterEventBus(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.RegisterOptions<RabbitMqOptions>(configuration);
        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<UserEventConsumer>();

            configurator.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                cfg.Host(host: options.Host, port: options.Port, virtualHost: options.VirtualHost, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });
                
                cfg.ReceiveEndpoint($"{nameof(UserService)}Queue", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.DefaultContentType = new ContentType("application/json");
                    e.UseRawJsonDeserializer();
                    e.Bind("amq.topic", ex =>
                    {
                        ex.ExchangeType = ExchangeType.Topic;
                        ex.RoutingKey = "KK.EVENT.*.app-dev.#";
                    });
                    e.ConfigureConsumer<UserEventConsumer>(context);
                });
            });
        });
        return services;
    }
}