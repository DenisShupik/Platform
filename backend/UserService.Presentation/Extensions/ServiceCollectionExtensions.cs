using System.Net.Mime;
using System.Text.Json;
using Common.Extensions;
using Common.Options;
using MassTransit;
using Microsoft.Extensions.Options;
using UserService.Application.Events;
using UserService.Infrastructure.Converters;
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
            configurator.AddConsumer<UserConsumer>();
    
            configurator.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                cfg.Host(host:options.Host,port:options.Port,virtualHost:options.VirtualHost, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });
                
                cfg.ConfigureJsonSerializerOptions(jsonSerializerOptions =>
                {
                    jsonSerializerOptions.Converters.Add(new UpperCaseEnumConverter<UserEvent.ResourceTypes>());
                    jsonSerializerOptions.Converters.Add(new UpperCaseEnumConverter<UserEvent.OperationTypes>());
                    
                    jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                    return jsonSerializerOptions;
                });
                

                cfg.ReceiveEndpoint($"{nameof(UserService)}Queue", e =>
                {
                    e.DefaultContentType = new ContentType("application/json");
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<UserConsumer>(context);
                });
            });
        });
        return services;
    }
}