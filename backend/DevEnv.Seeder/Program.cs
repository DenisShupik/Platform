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
    .RegisterOptions<KeycloakOptions, KeycloakOptionsValidator>(builder.Configuration);

var apiGatewayUri = builder.Configuration.GetValue<Uri>("services:api-gateway:http:0");

builder.Services.AddSingleton<Fixture>();
builder.Services.AddSingleton<UserTokenService>();
builder.Services.AddSingleton<ServiceTokenService>();

builder.Services.AddTransient<ServiceTokenService.Handler>();
builder.Services.AddHttpClient<KeycloakAdminClient>()
    .AddHttpMessageHandler<ServiceTokenService.Handler>();

builder.Services.AddHttpClient("randomUser", httpClient =>
    {
        httpClient.BaseAddress = apiGatewayUri;
    })
    .ConfigurePrimaryHttpMessageHandler(sp =>
    {
        var fixture = sp.GetRequiredService<Fixture>();
        var userTokenService = sp.GetRequiredService<UserTokenService>();
        var handler = new UserTokenService.Handler(userTokenService, fixture.GetRandomUser);
        handler.InnerHandler = new HttpClientHandler();
        return handler;
    });

foreach (var i in Enumerable.Range(1, Fixture.UserCount))
{
    var name = $"user{i}";
    builder.Services
        .AddHttpClient(name, httpClient =>
        {
            httpClient.BaseAddress = apiGatewayUri;
        })
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