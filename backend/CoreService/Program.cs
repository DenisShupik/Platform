using CoreService.Application;
using CoreService.Domain.Events;
using CoreService.Infrastructure.Grpc.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using CoreService.Infrastructure;
using CoreService.Infrastructure.Persistence;
using CoreService.Presentation.Extensions;
using CoreService.Presentation.Filters;
using CoreService.Presentation.Grpc;
using CoreService.Presentation.Options;
using JasperFx.CodeGeneration;
using ProtoBuf.Grpc.Server;
using SharedKernel.Presentation.Extensions;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;
using SharedKernel.Presentation.Options;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton)
    .RegisterOptions<CoreServiceOptions, CoreServiceOptionsValidator>(builder.Configuration)
    .RegisterAuthenticationSchemes(builder.Configuration)
    ;

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);



builder.Services.RegisterSwaggerGen(options => { options.DocumentFilter<TypesDocumentFilter>(); });

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithLogging(logging => logging.AddOtlpExporter());

builder.AddApplicationServices();
builder.AddInfrastructureServices<CoreServiceOptions>();

// TODO: Следовало бы включить в DependencyInjection, но AddWolverine можно вызвать лишь раз и WolverineOptions нет возможности настроить идиоматично
builder.Services.AddWolverine(options =>
{
    var coreServiceOptions = builder.Configuration.GetSection(nameof(CoreServiceOptions)).Get<CoreServiceOptions>();
    if (coreServiceOptions == null) throw new ArgumentNullException(nameof(coreServiceOptions));

    var rabbitMqOptions = builder.Configuration.GetSection(nameof(RabbitMqOptions)).Get<RabbitMqOptions>();
    if (rabbitMqOptions == null) throw new ArgumentNullException(nameof(rabbitMqOptions));

    var rabbitMqUri =
        new Uri(
            $"amqp://{rabbitMqOptions.Username}:{rabbitMqOptions.Password}@{rabbitMqOptions.Host}:{rabbitMqOptions.Port}{rabbitMqOptions.VirtualHost}");

    const string serviceNamePrefix = "core_service_";

    options.PublishMessage<PostAddedEvent>().ToRabbitExchange(serviceNamePrefix + "events");

    options.UseRabbitMq(rabbitMqUri).AutoProvision();

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
    .UseSwagger()
    .UseSwaggerUI();

app
    .UseAuthentication()
    .UseAuthorization();

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

public sealed partial class Program;