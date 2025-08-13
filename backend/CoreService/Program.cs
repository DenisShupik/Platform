using CoreService.Application;
using CoreService.Domain.Events;
using CoreService.Infrastructure.Grpc.Contracts;
using CoreService.Infrastructure;
using CoreService.Infrastructure.Options;
using CoreService.Infrastructure.Persistence;
using CoreService.Presentation;
using CoreService.Presentation.Extensions;
using CoreService.Presentation.Grpc;
using JasperFx.CodeGeneration;
using ProtoBuf.Grpc.Server;
using SharedKernel.Presentation.Extensions;
using SharedKernel.Infrastructure.Options;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices<CoreServiceOptions>();
builder.AddPresentationServices();

// TODO: Следовало бы включить в DependencyInjection, но AddWolverine можно вызвать лишь раз и WolverineOptions нет возможности настроить идиоматично
builder.Services.AddWolverine(options =>
{
    var coreServiceOptions = builder.Configuration.GetSection(nameof(CoreServiceOptions)).Get<CoreServiceOptions>();
    ArgumentNullException.ThrowIfNull(coreServiceOptions);

    var rabbitMqOptions = builder.Configuration.GetSection(nameof(RabbitMqOptions)).Get<RabbitMqOptions>();
    ArgumentNullException.ThrowIfNull(rabbitMqOptions);

    const string serviceNamePrefix = "core_service_";
    const string serviceExchangeName = serviceNamePrefix + "events";

    options.PublishMessage<PostAddedEvent>().ToRabbitExchange(serviceExchangeName);
    options.PublishMessage<PostUpdatedEvent>().ToRabbitExchange(serviceExchangeName);

    options.UseRabbitMq(factory =>
        {
            factory.HostName = rabbitMqOptions.Host;
            factory.Port = rabbitMqOptions.Port;
            factory.VirtualHost = rabbitMqOptions.VirtualHost;
            factory.UserName = rabbitMqOptions.Username;
            factory.Password = rabbitMqOptions.Password;
        })
        .AutoProvision();

    options.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);
    options.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;
    options.PersistMessagesWithPostgresql(coreServiceOptions.WritableConnectionString, serviceNamePrefix + "wolverine");
    options.UseEntityFrameworkCoreTransactions();
    options.Policies.UseDurableOutboxOnAllSendingEndpoints();
});

var app = builder.Build();

await app.ApplyMigrations<WritableApplicationDbContext>();

app
    .UseExceptionHandler()
    .UseAuthentication()
    .UseAuthorization();

app
    .UseSwagger()
    .UseSwaggerUI();

app.MapApi();

app.MapGrpcService<GrpcCoreService>();
app.MapCodeFirstGrpcReflectionService();
var schemaGenerator = new ProtoBuf.Grpc.Reflection.SchemaGenerator
{
    ProtoSyntax = ProtoBuf.Meta.ProtoSyntax.Proto3
};
var schema = schemaGenerator.GetSchema<IGrpcCoreService>();
Console.WriteLine(schema);

app.Logger.StartingApp();

await app.RunAsync();

namespace CoreService
{
    public sealed partial class Program;
}