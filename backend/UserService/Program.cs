using System.Text.Json.Nodes;
using JasperFx.CodeGeneration;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Server;
using SharedKernel.Infrastructure.Options;
using SharedKernel.Presentation.Extensions;
using UserService.Application;
using UserService.Infrastructure.Grpc.Contracts;
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

    // options.PublishMessage<UserUpdatedDomainEvent>()
    //     .
    
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
});

var app = builder.Build();

await app.ApplyMigrations<WritableApplicationDbContext>();

app
    .UseExceptionHandler()
    .UseAuthentication()
    .UseAuthorization()
    ;

app
    .UseSwagger()
    .UseSwaggerUI(options =>
    {
        var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;
        options.OAuthClientId(keycloakOptions.Audience);
        options.OAuthUsePkce();
    });

app.MapUserApi();

app.MapGrpcService<GrpcUserService>();
app.MapCodeFirstGrpcReflectionService();
var schemaGenerator = new ProtoBuf.Grpc.Reflection.SchemaGenerator
{
    ProtoSyntax = ProtoBuf.Meta.ProtoSyntax.Proto3
};
var schema = schemaGenerator.GetSchema<IGrpcUserService>();
Console.WriteLine(schema);

app.Logger.StartingApp();

await app.RunAsync();