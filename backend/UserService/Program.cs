using System.Text.Json.Nodes;
using JasperFx.CodeGeneration;
using ProtoBuf.Grpc.Server;
using Shared.Infrastructure.Options;
using Shared.Presentation.Authorization;
using Shared.Presentation.Extensions;
using UserService.Application;
using UserService.Infrastructure;
using UserService.Infrastructure.Options;
using UserService.Infrastructure.Persistence;
using UserService.Presentation;
using UserService.Presentation.Grpc;
using UserService.Presentation.Rest;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices<UserServiceOptions>();
builder.AddPresentationServices();

builder.Services.AddWolverine(options =>
{
    var userServiceOptions = builder.Configuration.GetSection(nameof(UserServiceOptions))
        .Get<UserServiceOptions>();
    ArgumentNullException.ThrowIfNull(userServiceOptions);

    var rabbitMqOptions = builder.Configuration.GetSection(nameof(RabbitMqOptions)).Get<RabbitMqOptions>();
    ArgumentNullException.ThrowIfNull(rabbitMqOptions);

    var keycloakOptions = builder.Configuration.GetSection(nameof(KeycloakOptions)).Get<KeycloakOptions>();
    ArgumentNullException.ThrowIfNull(keycloakOptions);

    options.UseRabbitMq(factory =>
        {
            factory.HostName = rabbitMqOptions.Host;
            factory.Port = rabbitMqOptions.Port;
            factory.VirtualHost = rabbitMqOptions.VirtualHost;
            factory.UserName = rabbitMqOptions.Username;
            factory.Password = rabbitMqOptions.Password;
        })
        .AutoProvision();

    const string queueName = $"{nameof(UserService)}Queue";

    options.ListenToRabbitQueue(queueName)
        .DefaultIncomingMessage<JsonNode>();

    options.UseRabbitMq()
        .BindExchange("keycloak", b => { b.ExchangeType = ExchangeType.Topic; })
        .ToQueue(
            queueName,
            $"KK.EVENT.*.{keycloakOptions.Realm}.#"
        );

    options.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);
    options.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;
    options.CodeGeneration.AlwaysUseServiceLocationFor<WriteApplicationDbContext>();
});

var app = builder.Build();

await app.ApplyMigrations<WriteApplicationDbContext>();

app
    .UseApiLocalization()
    .UseExceptionHandler()
    .UseAuthentication()
    .UseAuthorization()
    ;

app.MapOpenApi("/api/{documentName}.json");

app.MapServiceHealthChecks();
app.MapApi();

app.MapGrpcService<GrpcUserService>()
    .RequireAuthorization(AuthenticationPolicies.CoreServiceInternalApi);
if (app.Environment.IsDevelopment())
{
    app.MapCodeFirstGrpcReflectionService()
        .RequireAuthorization(AuthenticationPolicies.InternalApi);
}

app.Logger.StartingApp();

await app.RunAsync();

namespace UserService
{
    public sealed partial class Program;
}
