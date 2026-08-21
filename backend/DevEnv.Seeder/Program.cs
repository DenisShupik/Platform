using CoreService.Infrastructure.Grpc.Client;
using CoreService.Infrastructure.Grpc.Contracts;
using DevEnv.Seeder.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Options;
using Shared.Infrastructure.Services;
using Shared.Tests.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();

builder.Services
    .RegisterOptions<KeycloakOptions, KeycloakOptionsValidator>(builder.Configuration)
    .RegisterOptions<ServiceAccountOptions, ServiceAccountOptionsValidator>(builder.Configuration)
    .RegisterOptions<KeycloakAdminOptions, KeycloakAdminOptionsValidator>(builder.Configuration);

var apiGatewayUri = builder.Configuration.GetValue<Uri>("services:api-gateway:http:0");
var coreServiceGrpcUri = builder.Configuration.GetValue<Uri>("services:core-service:Grpc:0")
                         ?? throw new InvalidOperationException("CoreService gRPC endpoint is not configured.");

builder.Services.AddSingleton<Fixture>();
builder.Services.AddSingleton<UserTokenService>();
builder.Services.AddSingleton<KeycloakAdminTokenService>();
builder.Services.AddSingleton<ServiceTokenService>();

builder.Services.AddTransient<KeycloakAdminTokenService.Handler>();
builder.Services.AddTransient<ServiceTokenService.Handler>();
builder.Services.AddHttpClient<KeycloakAdminClient>()
    .AddHttpMessageHandler<KeycloakAdminTokenService.Handler>();

builder.Services.RegisterGrpcRuntimeTypeModel(model =>
{
    model.MapCoreServiceTypes();
    model.CompileInPlace();
});
builder.Services.AddCoreServiceGrpcClient(coreServiceGrpcUri)
    .AddHttpMessageHandler<ServiceTokenService.Handler>();

builder.Services.AddHttpClient("randomUser", httpClient => { httpClient.BaseAddress = apiGatewayUri; })
    .ConfigurePrimaryHttpMessageHandler(sp =>
    {
        var fixture = sp.GetRequiredService<Fixture>();
        var userTokenService = sp.GetRequiredService<UserTokenService>();
        var handler = new UserTokenService.Handler(userTokenService, fixture.GetRandomUser);
        handler.InnerHandler = new HttpClientHandler();
        return handler;
    });

builder.Services.AddHttpClient("admin", httpClient => { httpClient.BaseAddress = apiGatewayUri; })
    .ConfigurePrimaryHttpMessageHandler(sp =>
    {
        var userTokenService = sp.GetRequiredService<UserTokenService>();
        var handler = new UserTokenService.Handler(userTokenService, () => "admin");
        handler.InnerHandler = new HttpClientHandler();
        return handler;
    });

foreach (var i in Enumerable.Range(1, Fixture.UserCount))
{
    var name = $"user{i}";
    builder.Services
        .AddHttpClient(name, httpClient => { httpClient.BaseAddress = apiGatewayUri; })
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            var userTokenService = sp.GetRequiredService<UserTokenService>();
            var handler = new UserTokenService.Handler(userTokenService, () => name);
            handler.InnerHandler = new HttpClientHandler();
            return handler;
        });
}

builder.Services.AddHostedService<Seeder>();

var host = builder.Build();

await host.RunAsync();
