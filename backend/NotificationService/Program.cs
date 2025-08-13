using JasperFx.CodeGeneration;
using Microsoft.Extensions.Options;
using NotificationService.Application;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Options;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Presentation;
using NotificationService.Presentation.Extensions;
using SharedKernel.Presentation.Extensions;
using SharedKernel.Infrastructure.Options;
using TickerQ.DependencyInjection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices<NotificationServiceOptions>();
builder.AddPresentationServices();

// TODO: Следовало бы включить в DependencyInjection, но AddWolverine можно вызвать лишь раз и WolverineOptions нет возможности настроить идиоматично
builder.Services.AddWolverine(options =>
{
    var notificationServiceOptions = builder.Configuration.GetSection(nameof(NotificationServiceOptions))
        .Get<NotificationServiceOptions>();
    ArgumentNullException.ThrowIfNull(notificationServiceOptions);

    var rabbitMqOptions = builder.Configuration.GetSection(nameof(RabbitMqOptions)).Get<RabbitMqOptions>();
    ArgumentNullException.ThrowIfNull(rabbitMqOptions);

    const string serviceNamePrefix = "notification_service_";

    options.UseRabbitMq(factory =>
        {
            factory.HostName = rabbitMqOptions.Host;
            factory.Port = rabbitMqOptions.Port;
            factory.VirtualHost = rabbitMqOptions.VirtualHost;
            factory.UserName = rabbitMqOptions.Username;
            factory.Password = rabbitMqOptions.Password;
        })
        .AutoProvision();

    options.ListenToRabbitQueue(serviceNamePrefix + "incoming_events", q => { q.BindExchange("core_service_events"); });

    options.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);
    options.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;
    options.PersistMessagesWithPostgresql(notificationServiceOptions.WritableConnectionString,
        serviceNamePrefix + "wolverine");
    options.UseEntityFrameworkCoreTransactions();
    options.Policies.UseDurableInboxOnAllListeners();
});

var app = builder.Build();

await app.ApplyMigrations<WriteApplicationDbContext>();

app
    .UseExceptionHandler()
    .UseAuthentication()
    .UseAuthorization();

app.UseTickerQ();

app
    .UseSwagger()
    .UseSwaggerUI(options =>
    {
        var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;
        options.OAuthClientId(keycloakOptions.Audience);
        options.OAuthUsePkce();
    });

app.MapApi();

app.Logger.StartingApp();

await app.RunAsync();

public sealed partial class Program;