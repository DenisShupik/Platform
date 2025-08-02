using CoreService.Application;
using CoreService.Domain.Events;
using CoreService.Infrastructure.Grpc.Contracts;
using Microsoft.EntityFrameworkCore;
using CoreService.Infrastructure;
using CoreService.Infrastructure.Options;
using CoreService.Infrastructure.Persistence;
using CoreService.Presentation;
using CoreService.Presentation.Extensions;
using CoreService.Presentation.Grpc;
using JasperFx.CodeGeneration;
using ProtoBuf.Grpc.Server;
using SharedKernel.Infrastructure.Options;
using SharedKernel.Presentation.Extensions;
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
    if (coreServiceOptions == null) throw new ArgumentNullException(nameof(coreServiceOptions));

    var rabbitMqOptions = builder.Configuration.GetSection(nameof(RabbitMqOptions)).Get<RabbitMqOptions>();
    if (rabbitMqOptions == null) throw new ArgumentNullException(nameof(rabbitMqOptions));

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
    options.PersistMessagesWithPostgresql(coreServiceOptions.ConnectionString, serviceNamePrefix + "wolverine");
    options.UseEntityFrameworkCoreTransactions();
    options.Policies.UseDurableOutboxOnAllSendingEndpoints();
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