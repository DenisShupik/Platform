using JasperFx.CodeGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationService.Application;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Presentation;
using NotificationService.Presentation.Extensions;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using NotificationService.Presentation.Options;
using SharedKernel.Presentation.Extensions;
using SharedKernel.Presentation.Options;
using TickerQ.DependencyInjection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithLogging(logging => logging.AddOtlpExporter());

builder.AddApplicationServices();
builder.AddInfrastructureServices<NotificationServiceOptions>();
builder.AddPresentationServices();

// TODO: Следовало бы включить в DependencyInjection, но AddWolverine можно вызвать лишь раз и WolverineOptions нет возможности настроить идиоматично
builder.Services.AddWolverine(options =>
{
    var notificationServiceOptions = builder.Configuration.GetSection(nameof(NotificationServiceOptions))
        .Get<NotificationServiceOptions>();
    if (notificationServiceOptions == null) throw new ArgumentNullException(nameof(notificationServiceOptions));

    var rabbitMqOptions = builder.Configuration.GetSection(nameof(RabbitMqOptions)).Get<RabbitMqOptions>();
    if (rabbitMqOptions == null) throw new ArgumentNullException(nameof(rabbitMqOptions));
    
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

    options.ListenToRabbitQueue(serviceNamePrefix + "incoming_events", q =>
    {
        q.BindExchange("core_service_events");
    });
    
    options.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);
    options.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;
    options.PersistMessagesWithPostgresql(notificationServiceOptions.ConnectionString, serviceNamePrefix + "wolverine");
    options.UseEntityFrameworkCoreTransactions();
    options.Policies.UseDurableInboxOnAllListeners();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

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